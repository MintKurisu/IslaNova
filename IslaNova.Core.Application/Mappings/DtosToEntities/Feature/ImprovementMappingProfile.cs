using AutoMapper;
using IslaNova.Core.Application.Dtos.Feature;
using IslaNova.Core.Domain.Entities.Feature;

namespace IslaNova.Core.Application.Mappings.DtosToEntities.Feature
{
    public class ImprovementMappingProfile : Profile
    {
        public ImprovementMappingProfile()
        {
            CreateMap<Improvement, ImprovementDto>();
            CreateMap<ImprovementDto, Improvement>();
        }
    }
}
