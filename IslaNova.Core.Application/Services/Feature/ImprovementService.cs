using AutoMapper;
using IslaNova.Core.Application.Dtos.Feature;
using IslaNova.Core.Application.Interfaces.Feature;
using IslaNova.Core.Application.Services.Base;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Core.Domain.Interfaces.Feature;

namespace IslaNova.Core.Application.Services.Feature
{
    public class ImprovementService : GenericService<Improvement, ImprovementDto>, IImprovementService
    {
        public ImprovementService(IImprovementRepository repository, IMapper mapper)
            : base(repository, mapper)
        {

        }
    }
}
