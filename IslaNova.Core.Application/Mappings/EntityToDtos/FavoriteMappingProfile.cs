using AutoMapper;
using IslaNova.Core.Application.Dtos.Favorite;
using IslaNova.Core.Domain.Entities.UserInteraction;

namespace IslaNova.Core.Application.Mappings.EntityToDtos
{
    public class FavoriteMappingProfile : Profile
    {
        public FavoriteMappingProfile()
        {
            CreateMap<Favorite, FavoriteDto>();
            CreateMap<CreateFavoriteDto, Favorite>()
                .ForMember(dest => dest.FavoriteId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Property, opt => opt.Ignore());
        }
    }
}
