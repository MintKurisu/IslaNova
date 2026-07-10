namespace IslaNova.Core.Application.Interfaces.AI
{
    /// <summary>
    /// Abstracts the embedding generation. In production backed by OpenAI text-embedding-3-small.
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Generates a float[] vector representation of the given text.
        /// </summary>
        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
    }
}
