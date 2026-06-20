using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.Favorite;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Favorite.Commands.AddFavorite
{
    public class AddFavoriteCommand : IRequest<FavoriteDto?>
    {
        [SwaggerParameter(Description = "Client ID adding the favorite")]
        public string? ClientId { get; set; }
        [SwaggerParameter(Description = "Property ID to add as favorite")]
        public int PropertyId { get; set; }
    }

    public class AddFavoriteCommandHandler : IRequestHandler<AddFavoriteCommand, FavoriteDto?>
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IMapper _mapper;

        public AddFavoriteCommandHandler(
            IFavoriteRepository favoriteRepository,
            IMapper mapper)
        {
            _favoriteRepository = favoriteRepository;
            _mapper = mapper;
        }

        public async Task<FavoriteDto?> Handle(AddFavoriteCommand command, CancellationToken cancellationToken)
        {
            var exists = await _favoriteRepository
                .GetAllQuery()
                .AnyAsync(f => f.ClientId == command.ClientId && f.PropertyId == command.PropertyId, cancellationToken);

            if (exists) return null;

            var favorite = new Domain.Entities.UserInteraction.Favorite
            {
                ClientId = command.ClientId ?? "",
                PropertyId = command.PropertyId,
                CreatedAt = DateTime.UtcNow
            };

            var createdFavorite = await _favoriteRepository.AddAsync(favorite);
            return _mapper.Map<FavoriteDto>(createdFavorite);
        }
    }
}