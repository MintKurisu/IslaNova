using IslaNova.Core.Domain.Entities.AI;
using IslaNova.Core.Domain.Interfaces.Base;

namespace IslaNova.Core.Domain.Interfaces.AI
{
    public interface IPropertyEmbeddingRepository : IGenericRepository<PropertyEmbedding>
    {
        /// <summary>Gets an embedding by its associated PropertyId (unique).</summary>
        Task<PropertyEmbedding?> GetByPropertyIdAsync(int propertyId, CancellationToken ct = default);

        /// <summary>
        /// Performs a cosine similarity search against all stored property embeddings.
        /// Returns the top-K most similar entries above the given threshold.
        /// </summary>
        Task<List<(int PropertyId, string PlainText, double Similarity)>> SearchSimilarAsync(
            float[] queryEmbedding,
            int topK = 5,
            double threshold = 0.70,
            CancellationToken ct = default);
    }
}
