using IslaNova.Core.Application.Interfaces.AI;
using IslaNova.Core.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;

namespace IslaNova.Infrastructure.AI.Services
{
    /// <summary>
    /// Generates embeddings using OpenAI text-embedding-3-small model.
    /// Returns a 1536-dimension float[] compatible with the pgvector column.
    /// </summary>
    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _client;
        private readonly OpenAISettings _settings;
        private readonly ILogger<OpenAIEmbeddingService> _logger;

        public OpenAIEmbeddingService(IOptions<OpenAISettings> settings, ILogger<OpenAIEmbeddingService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            _client = new EmbeddingClient(_settings.EmbeddingModel, _settings.ApiKey);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty for embedding generation.", nameof(text));

            _logger.LogDebug("Generating embedding for text of length {Length}", text.Length);

            var response = await _client.GenerateEmbeddingAsync(text, cancellationToken: ct);
            var embedding = response.Value;

            // Convert ReadOnlyMemory<float> to float[]
            return embedding.ToFloats().ToArray();
        }
    }
}
