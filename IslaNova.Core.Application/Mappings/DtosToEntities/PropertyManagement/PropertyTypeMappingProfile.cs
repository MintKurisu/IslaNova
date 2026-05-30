using AutoMapper;
using IslaNova.Core.Application.Dtos.PropertyManagement.PropertyType;
using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Core.Application.Mappings.DtosToEntities.PropertyManagement
{
    public class PropertyTypeMappingProfile : Profile
    {
        public PropertyTypeMappingProfile()
        {
            CreateMap<PropertyType, PropertyTypeDto>()
                .ForMember(dest => dest.PropertyCount,
                    opt => opt.MapFrom(src => src.Properties != null ? src.Properties.Count : 0));

            CreateMap<PropertyTypeDto, PropertyType>();

            CreateMap<PropertyType, PropertyTypeApiDto>()
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());
        }
    }
}
