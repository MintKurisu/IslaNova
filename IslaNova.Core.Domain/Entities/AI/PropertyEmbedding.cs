using System.ComponentModel.DataAnnotations.Schema;

namespace IslaNova.Core.Domain.Entities.AI
{
    /// <summary>
    /// Stores the semantic text and pgvector embedding for a Property.
    /// The Embedding float[] is marked [NotMapped] — the actual vector(1536)
    /// column is managed via raw SQL in PropertyEmbeddingRepository.
    /// </summary>
    public class PropertyEmbedding
    {
        public int Id { get; set; }

        /// <summary>Foreign key to the Property (relational DB side).</summary>
        public int PropertyId { get; set; }

        /// <summary>Human-readable text used to generate the embedding.</summary>
        public required string PlainText { get; set; }

        /// <summary>
        /// The 1536-dimensional float vector (text-embedding-3-small).
        /// NOT mapped by EF Core — persisted and queried via raw SQL.
        /// </summary>
        [NotMapped]
        public float[]? Embedding { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
