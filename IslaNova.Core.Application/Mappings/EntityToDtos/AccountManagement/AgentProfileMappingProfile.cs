using AutoMapper;
using IslaNova.Core.Application.Dtos.AgentProfile;
using IslaNova.Core.Domain.Entities.AccountManagement;

namespace IslaNova.Core.Application.Mappings.EntityToDtos.AccountManagement
{
    public class AgentProfileMappingProfile : Profile
    {
        public AgentProfileMappingProfile()
        {
            CreateMap<AgentProfile, AgentProfileDto>();
        }
    }
}
