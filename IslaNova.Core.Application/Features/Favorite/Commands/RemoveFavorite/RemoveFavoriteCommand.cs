using IslaNova.Core.Domain.Interfaces.UserInteraction;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Favorite.Commands.RemoveFavorite
{
    public class RemoveFavoriteCommand : IRequest<bool>
    {
        [SwaggerParameter(Description = "Client ID removing the favorite")]
        public string? ClientId { get; set; }
        [SwaggerParameter(Description = "Property ID to remove from favorites")]
        public int PropertyId { get; set; }
    }

    public class RemoveFavoriteCommandHandler : IRequestHandler<RemoveFavoriteCommand, bool>
    {
        private readonly IFavoriteRepository _favoriteRepository;

        public RemoveFavoriteCommandHandler(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        public async Task<bool> Handle(RemoveFavoriteCommand command, CancellationToken cancellationToken)
        {
            var favorite = await _favoriteRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(f => f.ClientId == command.ClientId && f.PropertyId == command.PropertyId, cancellationToken);

            if (favorite == null) return false;

            await _favoriteRepository.DeleteAsync(favorite.FavoriteId);
            return true;
        }
    }
}
