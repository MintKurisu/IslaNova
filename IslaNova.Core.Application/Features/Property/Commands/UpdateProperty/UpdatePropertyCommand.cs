using AutoMapper;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.Feature;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Property.Commands.UpdateProperty
{
    public class UpdatePropertyCommand : IRequest<PropertyDto?>
    {
        [SwaggerSchema(ReadOnly = true)]
        public int PropertyId { get; set; }
        public int PropertyTypeId { get; set; }
        public int SaleTypeId { get; set; }
        public decimal Price { get; set; }
        public double LandSize { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string? Description { get; set; }
        public List<int>? ImprovementIds { get; set; }
        public List<string>? ImageUrls { get; set; }
    }

    public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, PropertyDto?>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public UpdatePropertyCommandHandler(
            IPropertyRepository propertyRepository,
            IPropertyImageRepository propertyImageRepository,
            IPropertyImprovementRepository propertyImprovementRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<PropertyDto?> Handle(UpdatePropertyCommand command, CancellationToken cancellationToken)
        {
            var property = await _propertyRepository.GetByIdAsync(command.PropertyId);
            if (property == null) return null;

            property.PropertyTypeId = command.PropertyTypeId;
            property.SaleTypeId = command.SaleTypeId;
            property.Price = command.Price;
            property.LandSize = command.LandSize;
            property.Bedrooms = command.Bedrooms;
            property.Bathrooms = command.Bathrooms;
            property.Description = command.Description ?? "";

            await _propertyRepository.UpdateAsync(property.PropertyId, property);

            if (command.ImageUrls != null)
            {
                var currentImages = await _propertyImageRepository
                    .GetAllQuery()
                    .Where(img => img.PropertyId == command.PropertyId)
                    .ToListAsync(cancellationToken);

                foreach (var img in currentImages)
                    await _propertyImageRepository.DeleteAsync(img.PropertyImageId);

                if (command.ImageUrls.Any())
                {
                    var images = command.ImageUrls.Select(url => new PropertyImage
                    {
                        PropertyId = command.PropertyId,
                        ImageUrl = url
                    }).ToList();

                    await _propertyImageRepository.AddRangeAsync(images);
                }
            }

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

            return dto;
        }
    }
}