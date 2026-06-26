using AutoMapper;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Core.Application.Mappings.EntityToDtos
{
    public class PropertyMappingProfile : Profile
    {
        public PropertyMappingProfile()
        {
            CreateMap<Property, PropertyDto>()
                .ForMember(dest => dest.PropertyTypeName, opt => opt.MapFrom(src => src.PropertyType != null ? src.PropertyType.Name : null))
                .ForMember(dest => dest.SaleTypeName, opt => opt.MapFrom(src => src.SaleType != null ? src.SaleType.Name : null))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(i => i.ImageUrl).ToList() : null))
                .ForMember(dest => dest.ImprovementNames, opt => opt.MapFrom(src => src.PropertyImprovements != null ? src.PropertyImprovements.Select(pi => pi.Improvement != null ? pi.Improvement.Name : "").ToList() : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.AgentName, opt => opt.Ignore())
                .ForMember(dest => dest.AgentEmail, opt => opt.Ignore())
                .ForMember(dest => dest.AgentPhone, opt => opt.Ignore())
                .ForMember(dest => dest.AgentProfileImage, opt => opt.Ignore());

            CreateMap<CreatePropertyDto, Property>()
                .ForMember(dest => dest.PropertyId, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyType, opt => opt.Ignore())
                .ForMember(dest => dest.SaleType, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyImprovements, opt => opt.Ignore())
                .ForMember(dest => dest.Offers, opt => opt.Ignore());

            CreateMap<UpdatePropertyDto, Property>()
                .ForMember(dest => dest.PropertyId, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.AgentId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyType, opt => opt.Ignore())
                .ForMember(dest => dest.SaleType, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyImprovements, opt => opt.Ignore())
                .ForMember(dest => dest.Offers, opt => opt.Ignore());


            CreateMap<Property, PropertyApiDto>()
            .ForMember(dest => dest.PropertyTypeName,
                opt => opt.MapFrom(src => src.PropertyType != null ? src.PropertyType.Name : string.Empty))
            .ForMember(dest => dest.SaleTypeName,
                opt => opt.MapFrom(src => src.SaleType != null ? src.SaleType.Name : string.Empty))
            .ForMember(dest => dest.ImprovementNames,
                opt => opt.MapFrom(src =>
                    src.PropertyImprovements != null
                        ? src.PropertyImprovements.Select(pi => pi.Improvement.Name).ToList()
                        : new List<string>()))
            .ForMember(dest => dest.AgentName, opt => opt.Ignore())
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }


}
