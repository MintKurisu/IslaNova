using AutoMapper;
using IslaNova.Core.Application.Dtos.PropertyManagement.SaleType;
using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Core.Application.Mappings.DtosToEntities.PropertyManagement
{
    public class SaleTypeMappingProfile : Profile
    {
        public SaleTypeMappingProfile()
        {
            CreateMap<SaleType, SaleTypeDto>()
                .ForMember(dest => dest.PropertyCount,
                    opt => opt.MapFrom(src => src.Properties != null ? src.Properties.Count : 0));

            CreateMap<SaleTypeDto, SaleType>();

            CreateMap<SaleType, SaleTypeApiDto>()
              .ReverseMap()
              .ForMember(dest => dest.Properties, opt => opt.Ignore());
        }
    }
}
