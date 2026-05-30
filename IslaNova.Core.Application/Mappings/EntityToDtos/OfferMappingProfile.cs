using AutoMapper;
using IslaNova.Core.Application.Dtos.Offer;
using IslaNova.Core.Domain.Entities.UserInteraction;

namespace IslaNova.Core.Application.Mappings.EntityToDtos
{
    public class OfferMappingProfile : Profile
    {
        public OfferMappingProfile()
        {
            CreateMap<Offer, OfferDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ClientName, opt => opt.Ignore());

            CreateMap<CreateOfferDto, Offer>()
                .ForMember(dest => dest.OfferId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Property, opt => opt.Ignore());
        }
    }
}
