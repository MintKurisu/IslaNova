using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.Feature;
using IslaNova.Core.Domain.Interfaces.Feature;

namespace IslaNova.Core.Application.Features.Improvement.Queries.GetAllImprovement
{
    /// <summary>
    /// Query used to retrieve all improvements available in the system.
    /// </summary>
    public class GetAllImprovementQuery : IRequest<IList<ImprovementDto>>
    {
    }

    public class GetAllImprovementQueryHandler : IRequestHandler<GetAllImprovementQuery, IList<ImprovementDto>>
    {
        private readonly IImprovementRepository _improvementRepository;
        private readonly IMapper _mapper;

        public GetAllImprovementQueryHandler(IImprovementRepository improvementRepository, IMapper mapper)
        {
            _improvementRepository = improvementRepository;
            _mapper = mapper;
        }

        public async Task<IList<ImprovementDto>> Handle(GetAllImprovementQuery query, CancellationToken cancellationToken)
        {
            var improvementList = await _improvementRepository.GetAllListAsync();

            return _mapper.Map<IList<ImprovementDto>>(improvementList);

        }
    }
}
