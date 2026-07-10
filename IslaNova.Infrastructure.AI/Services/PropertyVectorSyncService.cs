using IslaNova.Core.Application.Features.Property.Events;
using IslaNova.Core.Application.Helpers;
using IslaNova.Core.Application.Interfaces.AI;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.AI;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace IslaNova.Infrastructure.AI.Services
{
    /// <summary>
    /// Background service that listens to property lifecycle events and
    /// synchronizes the vector store (pgvector) asynchronously.
    ///
    /// Pattern: Channel&lt;T&gt; (in-memory queue) + IHostedService.
    /// - The Command Handlers publish events to the channel (fire-and-forget).
    /// - This service processes them in the background without blocking the HTTP request.
    /// - On failure, it retries with exponential backoff (up to 3 attempts).
    /// </summary>
    public class PropertyVectorSyncService : BackgroundService
    {
        private readonly Channel<PropertyVectorEvent> _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PropertyVectorSyncService> _logger;

        private const int MaxRetries = 3;

        public PropertyVectorSyncService(
            Channel<PropertyVectorEvent> channel,
            IServiceScopeFactory scopeFactory,
            ILogger<PropertyVectorSyncService> logger)
        {
            _channel = channel;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PropertyVectorSyncService started. Listening for vector sync events...");

            await foreach (var vectorEvent in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                await ProcessWithRetryAsync(vectorEvent, stoppingToken);
            }

            _logger.LogInformation("PropertyVectorSyncService stopped.");
        }

        private async Task ProcessWithRetryAsync(PropertyVectorEvent vectorEvent, CancellationToken ct)
        {
            var attempt = 0;
            while (attempt < MaxRetries)
            {
                attempt++;
                try
                {
                    await ProcessEventAsync(vectorEvent, ct);
                    return; // success
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    _logger.LogWarning("Vector sync cancelled during shutdown for PropertyId {PropertyId}.", vectorEvent.PropertyId);
                    return;
                }
                catch (Exception ex)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // 2s, 4s, 8s
                    _logger.LogWarning(ex,
                        "Vector sync failed (attempt {Attempt}/{Max}) for PropertyId {PropertyId}. Retrying in {Delay}s...",
                        attempt, MaxRetries, vectorEvent.PropertyId, delay.TotalSeconds);

                    if (attempt >= MaxRetries)
                    {
                        _logger.LogError(ex,
                            "Vector sync permanently failed after {Max} attempts for PropertyId {PropertyId}. Event: {EventType}",
                            MaxRetries, vectorEvent.PropertyId, vectorEvent.EventType);
                        return;
                    }

                    await Task.Delay(delay, ct);
                }
            }
        }

        private async Task ProcessEventAsync(PropertyVectorEvent vectorEvent, CancellationToken ct)
        {
            // Each event needs its own scope because IPropertyRepository, IslaNovaContext, etc. are Scoped
            using var scope = _scopeFactory.CreateScope();
            var services = scope.ServiceProvider;

            var embeddingRepository = services.GetRequiredService<IPropertyEmbeddingRepository>();
            var embeddingService    = services.GetRequiredService<IEmbeddingService>();

            switch (vectorEvent.EventType)
            {
                case VectorEventType.Deleted:
                    await HandleDeletedAsync(embeddingRepository, vectorEvent.PropertyId, ct);
                    break;

                case VectorEventType.Created:
                case VectorEventType.Updated:
                    await HandleUpsertAsync(services, embeddingRepository, embeddingService, vectorEvent, ct);
                    break;
            }
        }

        private async Task HandleDeletedAsync(
            IPropertyEmbeddingRepository embeddingRepository,
            int propertyId,
            CancellationToken ct)
        {
            await embeddingRepository.DeleteByPropertyIdAsync(propertyId, ct);
            _logger.LogInformation("Deleted vector embedding for PropertyId {PropertyId}.", propertyId);
        }

        private async Task HandleUpsertAsync(
            IServiceProvider services,
            IPropertyEmbeddingRepository embeddingRepository,
            IEmbeddingService embeddingService,
            PropertyVectorEvent vectorEvent,
            CancellationToken ct)
        {
            // Load property with all needed includes for text building
            var propertyRepository = services.GetRequiredService<IPropertyRepository>();
            var authService        = services.GetRequiredService<IAuthServiceForWebApi>();

            var property = await propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "PropertyImprovements.Improvement"])
                .FirstOrDefaultAsync(p => p.PropertyId == vectorEvent.PropertyId, ct);

            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found for vector sync. Skipping.", vectorEvent.PropertyId);
                return;
            }

            // Get agent name for richer semantic text
            string? agentName = null;
            try
            {
                var agent = await authService.GetUserById(property.AgentId);
                if (agent != null)
                    agentName = $"{agent.Name} {agent.LastName}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch agent info for PropertyId {PropertyId}. Continuing without agent name.", property.PropertyId);
            }

            // Build plain text → Generate embedding → Upsert (PostgreSQL ON CONFLICT)
            var plainText = PropertyTextBuilder.Build(property, agentName);

            _logger.LogInformation("Generating embedding for PropertyId {PropertyId}...", property.PropertyId);
            var embedding = await embeddingService.GenerateEmbeddingAsync(plainText, ct);

            await embeddingRepository.UpsertAsync(property.PropertyId, plainText, embedding, ct);

            _logger.LogInformation(
                "Upserted vector embedding for PropertyId {PropertyId} ({EventType}).",
                property.PropertyId, vectorEvent.EventType);
        }
    }
}
