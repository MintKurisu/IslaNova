using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.Feature;
using IslaNova.Core.Domain.Interfaces.Feature;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Improvement.Queries.GetImprovementById
{
    /// <summary>
    /// Query used to retrieve a single improvement by its unique identifier.
    /// </summary>
    public class GetImprovementByIdQuery : IRequest<ImprovementDto?>
    {
        /// <example>3</example>
        [SwaggerParameter(Description = "Unique identifier of the improvement")]
        public int ImprovementId { get; set; }
    }

    public class GetImprovementByIdQueryHandler : IRequestHandler<GetImprovementByIdQuery, ImprovementDto?>
    {

        private readonly IImprovementRepository _improvementRepository;
        private readonly IMapper _mapper;

        public GetImprovementByIdQueryHandler(IImprovementRepository improvementRepository, IMapper mapper)
        {
            _improvementRepository = improvementRepository;
            _mapper = mapper;
        }

        public async Task<ImprovementDto?> Handle(GetImprovementByIdQuery query, CancellationToken cancellationToken)
        {
            var improvement = await _improvementRepository.GetByIdAsync(query.ImprovementId);

            if (improvement == null)
            {
                return null;
            }

            return _mapper.Map<ImprovementDto>(improvement);
        }
    }
}
