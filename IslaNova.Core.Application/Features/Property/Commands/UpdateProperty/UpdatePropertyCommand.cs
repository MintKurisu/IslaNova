using AutoMapper;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Features.Property.Events;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Interfaces.Storage;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.Feature;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Channels;

namespace IslaNova.Core.Application.Features.Property.Commands.UpdateProperty
{
    public class UpdatePropertyCommand : IRequest<PropertyDto?>
    {
        [SwaggerSchema(ReadOnly = true)]
        public int PropertyId { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        public string? AgentId { get; set; }

        public int PropertyTypeId { get; set; }
        public int SaleTypeId { get; set; }
        public decimal Price { get; set; }
        public double LandSize { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string? Description { get; set; }
        public List<int>? ImprovementIds { get; set; }

        [SwaggerParameter(Description = "URLs de las imágenes existentes que se desean conservar")]
        public List<string>? ExistingImageUrls { get; set; }

        [SwaggerParameter(Description = "Nuevas imágenes a subir")]
        public List<IFormFile>? NewImagesFiles { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
    }

    public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, PropertyDto?>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;
        private readonly Channel<PropertyVectorEvent> _vectorChannel;

        public UpdatePropertyCommandHandler(
            IPropertyRepository propertyRepository,
            IPropertyImageRepository propertyImageRepository,
            IPropertyImprovementRepository propertyImprovementRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper,
            IStorageService storageService,
            Channel<PropertyVectorEvent> vectorChannel)
        {
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
            _authService = authService;
            _mapper = mapper;
            _storageService = storageService;
            _vectorChannel = vectorChannel;
        }

        public async Task<PropertyDto?> Handle(UpdatePropertyCommand command, CancellationToken cancellationToken)
        {
            var property = await _propertyRepository.GetByIdAsync(command.PropertyId);
            if (property == null) return null;

            if (property.AgentId != command.AgentId)
            {
                return null;
            }

            property.PropertyTypeId = command.PropertyTypeId;
            property.SaleTypeId = command.SaleTypeId;
            property.Price = command.Price;
            property.LandSize = command.LandSize;
            property.Bedrooms = command.Bedrooms;
            property.Bathrooms = command.Bathrooms;
            property.Description = command.Description ?? "";
            property.Latitude = command.Latitude;
            property.Longitude = command.Longitude;
            property.Address = command.Address;
            property.City = command.City;

            await _propertyRepository.UpdateAsync(property.PropertyId, property);

            // Images management
            var currentImages = await _propertyImageRepository
                .GetAllQuery()
                .Where(img => img.PropertyId == command.PropertyId)
                .ToListAsync(cancellationToken);

            var urlsToKeep = command.ExistingImageUrls ?? new List<string>();

            var imagesToDelete = currentImages
                .Where(img => !urlsToKeep.Contains(img.ImageUrl))
                .ToList();

            foreach (var img in imagesToDelete)
            {
                await _propertyImageRepository.DeleteAsync(img.PropertyImageId);
                await _storageService.DeleteAsync(img.ImageUrl, "property-images");
            }

            if (command.NewImagesFiles != null && command.NewImagesFiles.Any())
            {
                var fileName = Guid.NewGuid().ToString();
                var uploadedUrls = await _storageService.UploadMultipleAsync(
                    command.NewImagesFiles, "property-images", "properties", fileName);

                if (uploadedUrls != null && uploadedUrls.Any())
                {
                    var newImages = uploadedUrls.Select(url => new PropertyImage
                    {
                        PropertyId = command.PropertyId,
                        ImageUrl = url
                    }).ToList();

                    await _propertyImageRepository.AddRangeAsync(newImages);
                }
            }


            // Improvement Management
            if (command.ImprovementIds != null)
            {
                var currentImprovements = await _propertyImprovementRepository
                    .GetAllQuery()
                    .Where(pi => pi.PropertyId == command.PropertyId)
                    .ToListAsync(cancellationToken);

                foreach (var improvement in currentImprovements)
                    await _propertyImprovementRepository.DeleteAsync(improvement.PropertyImprovementId);

                if (command.ImprovementIds.Any())
                {
                    var improvements = command.ImprovementIds.Select(improvementId => new PropertyImprovement
                    {
                        PropertyId = command.PropertyId,
                        ImprovementId = improvementId
                    }).ToList();

                    await _propertyImprovementRepository.AddRangeAsync(improvements);
                }
            }

            var result = await _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement"])
                .FirstOrDefaultAsync(p => p.PropertyId == command.PropertyId, cancellationToken);

            if (result == null) return null;

            var dto = _mapper.Map<PropertyDto>(result);

            var agent = await _authService.GetUserById(result.AgentId);
            if (agent != null)
            {
                dto.AgentName = $"{agent.Name} {agent.LastName}";
                dto.AgentEmail = agent.Email;
                dto.AgentPhone = agent.PhoneNumber;
                dto.AgentProfileImage = agent.ProfileImage;
            }

            await _vectorChannel.Writer.WriteAsync(new PropertyVectorEvent
            {
                EventType = VectorEventType.Updated,
                PropertyId = command.PropertyId
            }, cancellationToken);

            return dto;
        }
    }
}