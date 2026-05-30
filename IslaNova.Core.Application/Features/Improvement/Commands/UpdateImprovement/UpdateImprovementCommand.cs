using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.Feature;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Domain.Interfaces.Feature;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Improvement.Commands.UpdateImprovement
{
    /// <summary>
    /// Command used to update an existing  improvement (feature) that it's associated with a property.
    /// </summary>
    public class UpdateImprovementCommand : IRequest<ImprovementDto?>
    {
        [SwaggerParameter(Description = "The unique identifier of the improvement to update")]
        [SwaggerSchema(ReadOnly = true)]
        public int ImprovementId { get; set; }

        /// <example>Swimming Pool</example>
        [SwaggerParameter(Description = "Name of the improvement to create.")]
        public string? Name { get; set; }

        /// <example>Large outdoor pool with night lighting.</example>
        [SwaggerParameter(Description = "A brief description of the improvement.")]
        public string? Description { get; set; }
    }

    public class UpdateImprovementCommandHandler : IRequestHandler<UpdateImprovementCommand, ImprovementDto?>
    {
        private readonly IImprovementRepository _improvementRepository;
        private readonly IMapper _mapper;

        public UpdateImprovementCommandHandler(IImprovementRepository improvementRepository, IMapper mapper)
        {
            _improvementRepository = improvementRepository;
            _mapper = mapper;
        }

        public async Task<ImprovementDto?> Handle(UpdateImprovementCommand command, CancellationToken cancellationToken)
        {

            var entity = await _improvementRepository.GetByIdAsync(command.ImprovementId);

            if (entity == null)
                throw new ApiException("Improvement not found", 400);

            Domain.Entities.Feature.Improvement newEntity = new()
            {
                ImprovementId = command.ImprovementId,
                Name = command.Name ?? "",
                Description = command.Description ?? "",
            };

            var response = await _improvementRepository.UpdateAsync(command.ImprovementId, newEntity);
            return _mapper.Map<ImprovementDto>(response);
        }
    }
}