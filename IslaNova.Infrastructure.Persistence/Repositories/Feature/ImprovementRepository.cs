using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Core.Domain.Interfaces.Feature;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Base;

namespace IslaNova.Infrastructure.Persistence.Repositories.Feature
{
    public class ImprovementRepository : GenericRepository<Improvement>, IImprovementRepository
    {
        public ImprovementRepository(IslaNovaContext context) : base(context)
        {

        }
    }
}
