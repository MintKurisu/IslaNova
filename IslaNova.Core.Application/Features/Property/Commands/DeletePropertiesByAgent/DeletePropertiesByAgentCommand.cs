using IslaNova.Core.Domain.Interfaces.Feature;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Property.Commands.DeletePropertiesByAgent
{
    public class DeletePropertiesByAgentCommand : IRequest<Unit>
    {
        [SwaggerParameter(Description = "Agent ID whose properties will be deleted")]
        public string? AgentId { get; set; }
    }

    public class DeletePropertiesByAgentCommandHandler : IRequestHandler<DeletePropertiesByAgentCommand, Unit>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;

        public DeletePropertiesByAgentCommandHandler(
            IPropertyRepository propertyRepository,
            IPropertyImageRepository propertyImageRepository,
            IPropertyImprovementRepository propertyImprovementRepository)
        {
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
        }

        public async Task<Unit> Handle(DeletePropertiesByAgentCommand command, CancellationToken cancellationToken)
        {
            var properties = await _propertyRepository
                .GetAllQuery()
                .Where(p => p.AgentId == command.AgentId)
                .ToListAsync(cancellationToken);

            foreach (var property in properties)
            {
                var images = await _propertyImageRepository
                    .GetAllQuery()
                    .Where(img => img.PropertyId == property.PropertyId)
                    .ToListAsync(cancellationToken);

                foreach (var image in images)
                    await _propertyImageRepository.DeleteAsync(image.PropertyImageId);

                var improvements = await _propertyImprovementRepository
                    .GetAllQuery()
                    .Where(pi => pi.PropertyId == property.PropertyId)
                    .ToListAsync(cancellationToken);

                foreach (var improvement in improvements)
                    await _propertyImprovementRepository.DeleteAsync(improvement.PropertyImprovementId);

                await _propertyRepository.DeleteAsync(property.PropertyId);
            }

            return Unit.Value;
        }
    }
}