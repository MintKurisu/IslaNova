using IslaNova.Core.Application.Dtos.Favorite;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Base;

namespace IslaNova.Core.Application.Interfaces.Favorite
{
    public interface IFavoriteService : IGenericService<FavoriteDto>
    {
        Task<bool> IsFavoriteAsync(string clientId, int propertyId);
        Task<FavoriteDto?> AddFavoriteAsync(CreateFavoriteDto dto);
        Task<bool> RemoveFavoriteAsync(string clientId, int propertyId);
        Task<List<PropertyDto>> GetClientFavoritePropertiesAsync(string clientId);
    }
}
