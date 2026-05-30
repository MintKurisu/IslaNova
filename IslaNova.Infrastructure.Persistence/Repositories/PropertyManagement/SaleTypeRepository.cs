using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Base;

namespace IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement
{
    public class SaleTypeRepository : GenericRepository<SaleType>, ISaleTypeRepository
    {
        public SaleTypeRepository(IslaNovaContext context) : base(context)
        {

        }
    }
}
