using IslaNova.Core.Domain.Entities.UserInteraction;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Base;

namespace IslaNova.Infrastructure.Persistence.Repositories.UserInteraction
{
    public class FavoriteRepository : GenericRepository<Favorite>, IFavoriteRepository
    {
        public FavoriteRepository(IslaNovaContext context) : base(context)
        {
        }
    }
}
