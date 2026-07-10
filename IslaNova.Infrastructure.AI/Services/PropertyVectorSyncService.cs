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
            // Each event needs its own scope because IPropertyRepository, IslaNovaContext etc. are Scoped
            using var scope = _scopeFactory.CreateScope();
            var services = scope.ServiceProvider;

            var embeddingRepository = services.GetRequiredService<IPropertyEmbeddingRepository>();
            var embeddingService = services.GetRequiredService<IEmbeddingService>();

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
            var existing = await embeddingRepository.GetByPropertyIdAsync(propertyId, ct);
            if (existing != null)
            {
                await embeddingRepository.DeleteAsync(existing.Id);
                _logger.LogInformation("Deleted vector embedding for PropertyId {PropertyId}.", propertyId);
            }
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
            var authService = services.GetRequiredService<IAuthServiceForWebApi>();

            var property = await propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "PropertyImprovements.Improvement"])
                .FirstOrDefaultAsync(p => p.PropertyId == vectorEvent.PropertyId, ct);

            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found for vector sync. Skipping.", vectorEvent.PropertyId);
                return;
            }

            // Get agent name for richer text
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

            // Build plain text
            var plainText = PropertyTextBuilder.Build(property, agentName);

            // Generate embedding
            _logger.LogInformation("Generating embedding for PropertyId {PropertyId}...", property.PropertyId);
            var embedding = await embeddingService.GenerateEmbeddingAsync(plainText, ct);

            // Upsert in the vector store
            var existing = await embeddingRepository.GetByPropertyIdAsync(property.PropertyId, ct);

            if (existing == null)
            {
                // Create new
                var newEmbedding = new Core.Domain.Entities.AI.PropertyEmbedding
                {
                    PropertyId = property.PropertyId,
                    PlainText = plainText,
                    Embedding = embedding,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await embeddingRepository.AddAsync(newEmbedding);
                _logger.LogInformation("Created vector embedding for PropertyId {PropertyId}.", property.PropertyId);
            }
            else
            {
                // Update existing
                existing.PlainText = plainText;
                existing.Embedding = embedding;
                existing.UpdatedAt = DateTime.UtcNow;
                await embeddingRepository.UpdateAsync(existing.Id, existing);
                _logger.LogInformation("Updated vector embedding for PropertyId {PropertyId}.", property.PropertyId);
            }
        }
    }
}
