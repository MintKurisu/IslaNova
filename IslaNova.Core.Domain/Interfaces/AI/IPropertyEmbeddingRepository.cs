namespace IslaNova.Core.Domain.Interfaces.AI
{
    /// <summary>
    /// Repository for PropertyEmbeddings.
    /// All vector read/write operations use raw SQL (vector(1536) column is not EF-mapped).
    /// </summary>
    public interface IPropertyEmbeddingRepository
    {
        /// <summary>
        /// Inserts or updates the embedding for a property (PostgreSQL UPSERT ON CONFLICT).
        /// </summary>
        Task UpsertAsync(int propertyId, string plainText, float[] embedding, CancellationToken ct = default);

        /// <summary>
        /// Deletes the embedding row for a given property (when property is deleted).
        /// </summary>
        Task DeleteByPropertyIdAsync(int propertyId, CancellationToken ct = default);

        /// <summary>
        /// Finds the top-K most similar properties using cosine similarity on pgvector.
        /// </summary>
        Task<List<(int PropertyId, string PlainText, double Similarity)>> SearchSimilarAsync(
            float[] queryEmbedding,
            int topK = 5,
            double threshold = 0.70,
            CancellationToken ct = default);
    }
}
