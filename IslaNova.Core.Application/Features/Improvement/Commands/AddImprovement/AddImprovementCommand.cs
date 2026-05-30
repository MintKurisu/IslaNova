using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.Feature;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Domain.Interfaces.Feature;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace IslaNova.Core.Application.Features.Improvement.Commands.AddImprovement
{
    /// <summary>
    /// Command used to create a new improvement (feature) that can be associated with a property.
    /// </summary>
    public class AddImprovementCommand : IRequest<ImprovementDto?>
    {
        /// <example>Swimming Pool</example>
        [SwaggerParameter(Description = "Name of the improvement to create.")]
        public string? Name { get; set; }

        /// <example>Large outdoor pool with night lighting.</example>
        [SwaggerParameter(Description = "A brief description of the improvement.")]
        public string? Description { get; set; }
    }

    public class AddImprovementCommandHandler : IRequestHandler<AddImprovementCommand, ImprovementDto?>
    {
        private readonly IImprovementRepository _improvementRepository;
        private readonly IMapper _mapper;

        public AddImprovementCommandHandler(IImprovementRepository improvementRepository, IMapper mapper)
        {
            _improvementRepository = improvementRepository;
            _mapper = mapper;
        }

        public async Task<ImprovementDto?> Handle(AddImprovementCommand command, CancellationToken cancellationToken)
        {
            Domain.Entities.Feature.Improvement entity = new()
            {
                Name = command.Name ?? "",
                Description = command.Description ?? "",
            };

            var response = await _improvementRepository.AddAsync(entity);

            if (response == null)
                throw new ApiException("Error creating Improvement", (int)HttpStatusCode.InternalServerError);

            return _mapper.Map<ImprovementDto>(response);
        }
    }
}
