using AutoMapper;
using IslaNova.Core.Application.Dtos.AgentApplication;
using IslaNova.Core.Domain.Entities.AccountManagement;

namespace IslaNova.Core.Application.Mappings.EntityToDtos.AccountManagement
{
    public class AgentApplicationMappingProfile : Profile
    {
        public AgentApplicationMappingProfile()
        {
            CreateMap<AgentApplication, AgentApplicationDto>()
                .ForMember(dest => dest.EmploymentType,
                    opt => opt.MapFrom(src => src.EmploymentType.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.AgentName, opt => opt.Ignore())
                .ForMember(dest => dest.AgentEmail, opt => opt.Ignore())
                .ForMember(dest => dest.AgentPhone, opt => opt.Ignore());
        }
    }
}
