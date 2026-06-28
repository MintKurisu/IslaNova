using AutoMapper;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Helpers;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.Feature;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace IslaNova.Core.Application.Features.Property.Commands.CreateProperty
{
    public class CreatePropertyCommand : IRequest<PropertyDto?>
    {
        [SwaggerParameter(Description = "Property type ID")]
        public int PropertyTypeId { get; set; }
        [SwaggerParameter(Description = "Sale type ID")]
        public int SaleTypeId { get; set; }
        [SwaggerParameter(Description = "Price in DOP")]
        public decimal Price { get; set; }
        [SwaggerParameter(Description = "Land size in square meters")]
        public double LandSize { get; set; }
        [SwaggerParameter(Description = "Number of bedrooms")]
        public int Bedrooms { get; set; }
        [SwaggerParameter(Description = "Number of bathrooms")]
        public int Bathrooms { get; set; }
        [SwaggerParameter(Description = "Property description")]
        public string? Description { get; set; }
        [SwaggerParameter(Description = "Agent ID")]

        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public string? AgentId { get; set; }
        [SwaggerParameter(Description = "List of image URLs")]
        public List<string>? ImageUrls { get; set; }
        [SwaggerParameter(Description = "List of improvement IDs")]
        public List<int>? ImprovementIds { get; set; }

        [SwaggerParameter(Description = "Property latitude coordinate")]
        public double? Latitude { get; set; }
        [SwaggerParameter(Description = "Property longitude coordinate")]
        public double? Longitude { get; set; }
        [SwaggerParameter(Description = "Property address")]
        public string? Address { get; set; }
        [SwaggerParameter(Description = "Property city")]
        public string? City { get; set; }
    }

    public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, PropertyDto?>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public CreatePropertyCommandHandler(
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

        public async Task<PropertyDto?> Handle(CreatePropertyCommand command, CancellationToken cancellationToken)
        {
            string code = RandomDigitSequenceHelper.Generate(6);
            while (await _propertyRepository.GetAllQuery().AnyAsync(p => p.Code == code, cancellationToken))
            {
                code = RandomDigitSequenceHelper.Generate(6);
            }

            var property = new Domain.Entities.PropertyManagement.Property
            {
                Code = code,
                PropertyTypeId = command.PropertyTypeId,
                SaleTypeId = command.SaleTypeId,
                Price = command.Price,
                LandSize = command.LandSize,
                Bedrooms = command.Bedrooms,
                Bathrooms = command.Bathrooms,
                Description = command.Description ?? "",
                AgentId = command.AgentId ?? "",
                Status = PropertyStatus.Available,
                CreatedAt = DateTime.UtcNow,
                Latitude = command.Latitude,
                Longitude = command.Longitude,
                Address = command.Address,
                City = command.City
            };

            var createdProperty = await _propertyRepository.AddAsync(property);
            if (createdProperty == null) return null;

            if (command.ImageUrls != null && command.ImageUrls.Any())
            {
                var images = command.ImageUrls.Select(url => new PropertyImage
                {
                    PropertyId = createdProperty.PropertyId,
                    ImageUrl = url
                }).ToList();

                await _propertyImageRepository.AddRangeAsync(images);
            }

            if (command.ImprovementIds != null && command.ImprovementIds.Any())
            {
                var improvements = command.ImprovementIds.Select(improvementId => new PropertyImprovement
                {
                    PropertyId = createdProperty.PropertyId,
                    ImprovementId = improvementId
                }).ToList();

                await _propertyImprovementRepository.AddRangeAsync(improvements);
            }

            var result = await _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement"])
                .FirstOrDefaultAsync(p => p.PropertyId == createdProperty.PropertyId, cancellationToken);

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