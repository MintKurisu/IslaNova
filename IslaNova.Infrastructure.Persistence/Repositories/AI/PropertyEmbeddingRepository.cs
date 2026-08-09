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

            // Column names use PascalCase — EF Core + Npgsql default (no snake_case mapping configured).
            // ON CONFLICT targets the unique index on "PropertyId".
            const string sql = """
                INSERT INTO "PropertyEmbeddings" ("PropertyId", "PlainText", embedding, "CreatedAt", "UpdatedAt")
                VALUES (@propertyId, @plainText, @embedding::vector, NOW(), NOW())
                ON CONFLICT ("PropertyId")
                DO UPDATE SET
                    "PlainText"  = EXCLUDED."PlainText",
                    embedding    = EXCLUDED.embedding,
                    "UpdatedAt"  = NOW()
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
                DELETE FROM "PropertyEmbeddings" WHERE "PropertyId" = @propertyId
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
                SELECT "PropertyId",
                       "PlainText",
                       1 - (embedding <=> '{vectorLiteral}'::vector) AS "Similarity"
                FROM   "PropertyEmbeddings"
                WHERE  embedding IS NOT NULL
                  AND  1 - (embedding <=> '{vectorLiteral}'::vector) >= {threshold.ToString("F10", CultureInfo.InvariantCulture)}
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
        /// Uses F9 (fixed-point, 9 decimal places) — NOT G9 — to guarantee that all float values
        /// are serialized in standard decimal notation. G9 can emit scientific notation (e.g. 1.23E-05)
        /// for small values common in embeddings, which pgvector's ::vector cast does NOT accept.
        /// F9 is safe because OpenAI embeddings are unit-normalized and always within [-1, 1].
        /// </summary>
        private static string FormatVector(float[] v) =>
            "[" + string.Join(",", v.Select(f => f.ToString("F9", CultureInfo.InvariantCulture))) + "]";

        // Projection type for SqlQueryRaw — property names MUST match column aliases in SQL
        private sealed class SearchResult
        {
            public int PropertyId { get; set; }
            public string PlainText { get; set; } = string.Empty;
            public double Similarity { get; set; }
        }
    }
}
