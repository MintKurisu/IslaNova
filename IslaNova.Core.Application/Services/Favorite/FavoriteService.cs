using AutoMapper;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.Favorite;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Favorite;
using IslaNova.Core.Application.Interfaces.Property;
using IslaNova.Core.Application.Services.Base;
using IslaNova.Core.Domain.Interfaces.UserInteraction;

namespace IslaNova.Core.Application.Services.Favorite
{
    public class FavoriteService : GenericService<Core.Domain.Entities.UserInteraction.Favorite, FavoriteDto>, IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IPropertyService _propertyService;
        private readonly IMapper _mapper;

        public FavoriteService(
            IFavoriteRepository favoriteRepository,
            IPropertyService propertyService,
            IMapper mapper) : base(favoriteRepository, mapper)
        {
            _favoriteRepository = favoriteRepository;
            _propertyService = propertyService;
            _mapper = mapper;
        }

        public async Task<bool> IsFavoriteAsync(string clientId, int propertyId)
        {
            try
            {
                return await _favoriteRepository
                    .GetAllQuery()
                    .AnyAsync(f => f.ClientId == clientId && f.PropertyId == propertyId);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<FavoriteDto?> AddFavoriteAsync(CreateFavoriteDto dto)
        {
            try
            {
                // Check if already exists
                var exists = await IsFavoriteAsync(dto.ClientId, dto.PropertyId);
                if (exists) return null;

                var favorite = new Core.Domain.Entities.UserInteraction.Favorite
                {
                    ClientId = dto.ClientId,
                    PropertyId = dto.PropertyId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdFavorite = await _favoriteRepository.AddAsync(favorite);
                return _mapper.Map<FavoriteDto>(createdFavorite);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> RemoveFavoriteAsync(string clientId, int propertyId)
        {
            try
            {
                var favorite = await _favoriteRepository
                    .GetAllQuery()
                    .FirstOrDefaultAsync(f => f.ClientId == clientId && f.PropertyId == propertyId);

                if (favorite == null) return false;

                await _favoriteRepository.DeleteAsync(favorite.FavoriteId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<PropertyDto>> GetClientFavoritePropertiesAsync(string clientId)
        {
            try
            {
                var favorites = await _favoriteRepository
                    .GetAllQuery()
                    .Where(f => f.ClientId == clientId)
                    .Select(f => f.PropertyId)
                    .ToListAsync();

                var properties = new List<PropertyDto>();

                foreach (var propertyId in favorites)
                {
                    var property = await _propertyService.GetPropertyWithDetailsAsync(propertyId);
                    if (property != null)
                    {
                        properties.Add(property);
                    }
                }

                return properties;
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}
