using IslaNova.Core.Application.Features.Property.Events;
using IslaNova.Core.Application.Interfaces.Storage;
using IslaNova.Core.Domain.Interfaces.Feature;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Channels;

namespace IslaNova.Core.Application.Features.Property.Commands.DeleteProperty
{
    public class DeletePropertyCommand : IRequest<Unit>
    {
        [SwaggerParameter(Description = "Property ID to delete")]
        public int PropertyId { get; set; }
    }

    public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, Unit>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IStorageService _storageService;
        private readonly Channel<PropertyVectorEvent> _vectorChannel;

        public DeletePropertyCommandHandler(
            IPropertyRepository propertyRepository,
            IPropertyImageRepository propertyImageRepository,
            IPropertyImprovementRepository propertyImprovementRepository,
            IStorageService storageService,
            Channel<PropertyVectorEvent> vectorChannel)
        {
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
            _storageService = storageService;
            _vectorChannel = vectorChannel;
        }

        public async Task<Unit> Handle(DeletePropertyCommand command, CancellationToken cancellationToken)
        {
            var images = await _propertyImageRepository
                .GetAllQuery()
                .Where(img => img.PropertyId == command.PropertyId)
                .ToListAsync(cancellationToken);

            foreach (var image in images)
            {
                await _propertyImageRepository.DeleteAsync(image.PropertyImageId);
                await _storageService.DeleteAsync(image.ImageUrl, "property-images");
            }

            var improvements = await _propertyImprovementRepository
                .GetAllQuery()
                .Where(pi => pi.PropertyId == command.PropertyId)
                .ToListAsync(cancellationToken);

            foreach (var improvement in improvements)
                await _propertyImprovementRepository.DeleteAsync(improvement.PropertyImprovementId);

            await _propertyRepository.DeleteAsync(command.PropertyId);

            // Publish async event to remove stale vector embedding (non-blocking)
            await _vectorChannel.Writer.WriteAsync(new PropertyVectorEvent
            {
                EventType = VectorEventType.Deleted,
                PropertyId = command.PropertyId
            }, cancellationToken);

            return Unit.Value;
        }
    }
}