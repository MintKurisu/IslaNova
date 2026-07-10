using System.Globalization;
using IslaNova.Core.Domain.Interfaces.AI;
using IslaNova.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IslaNova.Infrastructure.Persistence.Repositories.AI
{
    /// <summary>
    /// All vector operations use raw SQL with the pgvector <=> operator.
    /// The embedding column (vector(1536)) is NOT tracked by EF Core — it is managed here exclusively.
    /// </summary>
    public class PropertyEmbeddingRepository : IPropertyEmbeddingRepository
    {
        private readonly IslaNovaContext _context;

        public PropertyEmbeddingRepository(IslaNovaContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task UpsertAsync(int propertyId, string plainText, float[] embedding, CancellationToken ct = default)
        {
            // Format vector as a pgvector literal string: [x,y,z,...]
            var vectorLiteral = FormatVector(embedding);

            const string sql = """
                INSERT INTO "PropertyEmbeddings" (property_id, plain_text, embedding, created_at, updated_at)
                VALUES (@propertyId, @plainText, @embedding::vector, NOW(), NOW())
                ON CONFLICT (property_id)
                DO UPDATE SET
                    plain_text = EXCLUDED.plain_text,
                    embedding  = EXCLUDED.embedding,
                    updated_at = NOW()
                """;

            await _context.Database.ExecuteSqlRawAsync(sql,
                new object[]
                {
                    new NpgsqlParameter("@propertyId", propertyId),
                    new NpgsqlParameter("@plainText", plainText),
                    new NpgsqlParameter("@embedding", vectorLiteral)
                },
                ct);
        }

        /// <inheritdoc/>
        public async Task DeleteByPropertyIdAsync(int propertyId, CancellationToken ct = default)
        {
            const string sql = """
                DELETE FROM "PropertyEmbeddings" WHERE property_id = @propertyId
                """;

            await _context.Database.ExecuteSqlRawAsync(sql,
                new object[] { new NpgsqlParameter("@propertyId", propertyId) },
                ct);
        }

        /// <inheritdoc/>
        public async Task<List<(int PropertyId, string PlainText, double Similarity)>> SearchSimilarAsync(
            float[] queryEmbedding,
            int topK = 5,
            double threshold = 0.70,
            CancellationToken ct = default)
        {
            var vectorLiteral = FormatVector(queryEmbedding);

            // We use a safe threshold and topK — both are controlled by application code, not user input.
            // The vector literal is a formatted float array, not free-form user text.
            var sql = $"""
                SELECT property_id AS "PropertyId",
                       plain_text  AS "PlainText",
                       1 - (embedding <=> '{vectorLiteral}'::vector) AS "Similarity"
                FROM   "PropertyEmbeddings"
                WHERE  embedding IS NOT NULL
                  AND  1 - (embedding <=> '{vectorLiteral}'::vector) >= {threshold.ToString("G", CultureInfo.InvariantCulture)}
                ORDER  BY embedding <=> '{vectorLiteral}'::vector
                LIMIT  {topK}
                """;

            var results = await _context.Database
                .SqlQueryRaw<SearchResult>(sql)
                .ToListAsync(ct);

            return results
                .Select(r => (r.PropertyId, r.PlainText, r.Similarity))
                .ToList();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts float[] to a pgvector literal: [x,y,z,...]
        /// Uses G9 format for 9 significant digits — appropriate for float32 precision.
        /// </summary>
        private static string FormatVector(float[] v) =>
            "[" + string.Join(",", v.Select(f => f.ToString("G9", CultureInfo.InvariantCulture))) + "]";

        // Projection type for SqlQueryRaw — property names MUST match column aliases in SQL
        private sealed class SearchResult
        {
            public int PropertyId { get; set; }
            public string PlainText { get; set; } = string.Empty;
            public double Similarity { get; set; }
        }
    }
}
