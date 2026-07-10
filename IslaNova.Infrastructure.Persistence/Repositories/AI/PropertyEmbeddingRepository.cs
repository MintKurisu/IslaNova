using IslaNova.Core.Domain.Entities.AI;
using IslaNova.Core.Domain.Interfaces.AI;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace IslaNova.Infrastructure.Persistence.Repositories.AI
{
    public class PropertyEmbeddingRepository : GenericRepository<PropertyEmbedding>, IPropertyEmbeddingRepository
    {
        public PropertyEmbeddingRepository(IslaNovaContext context) : base(context) { }

        public async Task<PropertyEmbedding?> GetByPropertyIdAsync(int propertyId, CancellationToken ct = default)
        {
            return await _context.PropertyEmbeddings
                .FirstOrDefaultAsync(e => e.PropertyId == propertyId, ct);
        }

        public async Task<List<(int PropertyId, string PlainText, double Similarity)>> SearchSimilarAsync(
            float[] queryEmbedding,
            int topK = 5,
            double threshold = 0.70,
            CancellationToken ct = default)
        {
            var queryVector = new Pgvector.Vector(queryEmbedding);

            var results = await _context.PropertyEmbeddings
                .Where(e => e.Embedding != null)
                .OrderBy(e => e.Embedding!.CosineDistance(queryVector))
                .Take(topK * 2) // take more, then filter by threshold
                .Select(e => new
                {
                    e.PropertyId,
                    e.PlainText,
                    // Cosine similarity = 1 - cosine distance
                    Similarity = 1.0 - e.Embedding!.CosineDistance(queryVector)
                })
                .ToListAsync(ct);

            return results
                .Where(r => r.Similarity >= threshold)
                .Take(topK)
                .Select(r => (r.PropertyId, r.PlainText, r.Similarity))
                .ToList();
        }
    }
}
