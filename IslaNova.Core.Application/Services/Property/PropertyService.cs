using AutoMapper;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Helpers;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Interfaces.Property;
using IslaNova.Core.Application.Services.Base;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.Feature;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;

namespace IslaNova.Core.Application.Services.Property
{
    public class PropertyService : GenericService<Core.Domain.Entities.PropertyManagement.Property, PropertyDto>, IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public PropertyService(
            IPropertyRepository propertyRepository,
            IPropertyImageRepository propertyImageRepository,
            IPropertyImprovementRepository propertyImprovementRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper) : base(propertyRepository, mapper)
        {
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<List<PropertyDto>> GetAvailablePropertiesAsync()
        {
            try
            {
                var properties = await _propertyRepository
                    .GetAllQueryWithInclude(new List<string> { "PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement" })
                    .Where(p => p.Status == PropertyStatus.Available)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var propertyDtos = new List<PropertyDto>();

                foreach (var property in properties)
                {
                    var agent = await _authService.GetUserById(property.AgentId);
                    var dto = _mapper.Map<PropertyDto>(property);

                    if (agent != null)
                    {
                        dto.AgentName = $"{agent.Name} {agent.LastName}";
                        dto.AgentEmail = agent.Email;
                        dto.AgentPhone = agent.PhoneNumber;
                    }

                    propertyDtos.Add(dto);
                }

                return propertyDtos;
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<List<PropertyDto>> GetPropertiesByAgentIdAsync(string agentId)
        {
            try
            {
                var properties = await _propertyRepository
                    .GetAllQueryWithInclude(new List<string> { "PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement" })
                    .Where(p => p.AgentId == agentId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var agent = await _authService.GetUserById(agentId);
                var propertyDtos = new List<PropertyDto>();

                foreach (var property in properties)
                {
                    var dto = _mapper.Map<PropertyDto>(property);

                    if (agent != null)
                    {
                        dto.AgentName = $"{agent.Name} {agent.LastName}";
                        dto.AgentEmail = agent.Email;
                        dto.AgentPhone = agent.PhoneNumber;
                    }

                    propertyDtos.Add(dto);
                }

                return propertyDtos;
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<PropertyDto?> GetPropertyByCodeAsync(string code)
        {
            try
            {
                var property = await _propertyRepository
                    .GetAllQueryWithInclude(new List<string> { "PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement" })
                    .FirstOrDefaultAsync(p => p.Code == code);

                if (property == null) return null;

                var agent = await _authService.GetUserById(property.AgentId);
                var dto = _mapper.Map<PropertyDto>(property);

                if (agent != null)
                {
                    dto.AgentName = $"{agent.Name} {agent.LastName}";
                    dto.AgentEmail = agent.Email;
                    dto.AgentPhone = agent.PhoneNumber;
                }

                return dto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PropertyDto?> GetPropertyWithDetailsAsync(int id)
        {
            try
            {
                var property = await _propertyRepository
                    .GetAllQueryWithInclude(new List<string> { "PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement" })
                    .FirstOrDefaultAsync(p => p.PropertyId == id);

                if (property == null) return null;

                var agent = await _authService.GetUserById(property.AgentId);
                var dto = _mapper.Map<PropertyDto>(property);

                if (agent != null)
                {
                    dto.AgentName = $"{agent.Name} {agent.LastName}";
                    dto.AgentEmail = agent.Email;
                    dto.AgentPhone = agent.PhoneNumber;
                    dto.AgentProfileImage = agent.ProfileImage;
                }

                return dto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PropertyDto?> CreatePropertyAsync(CreatePropertyDto dto)
        {
            try
            {
                // Generate unique code
                string code = RandomDigitSequenceHelper.Generate(6);
                while (await _propertyRepository.GetAllQuery().AnyAsync(p => p.Code == code))
                {
                    code = RandomDigitSequenceHelper.Generate(6);
                }

                var property = new Core.Domain.Entities.PropertyManagement.Property
                {
                    Code = code,
                    PropertyTypeId = dto.PropertyTypeId,
                    SaleTypeId = dto.SaleTypeId,
                    Price = dto.Price,
                    LandSize = dto.LandSize,
                    Bedrooms = dto.Bedrooms,
                    Bathrooms = dto.Bathrooms,
                    Description = dto.Description,
                    AgentId = dto.AgentId,
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                };

                var createdProperty = await _propertyRepository.AddAsync(property);
                if (createdProperty == null) return null;

                // Add images
                if (dto.ImageUrls != null && dto.ImageUrls.Any())
                {
                    var images = dto.ImageUrls.Select(url => new PropertyImage
                    {
                        PropertyId = createdProperty.PropertyId,
                        ImageUrl = url
                    }).ToList();

                    await _propertyImageRepository.AddRangeAsync(images);
                }

                // Add improvements
                if (dto.ImprovementIds != null && dto.ImprovementIds.Any())
                {
                    var improvements = dto.ImprovementIds.Select(improvementId => new PropertyImprovement
                    {
                        PropertyId = createdProperty.PropertyId,
                        ImprovementId = improvementId
                    }).ToList();

                    await _propertyImprovementRepository.AddRangeAsync(improvements);
                }

                return await GetPropertyWithDetailsAsync(createdProperty.PropertyId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PropertyDto?> UpdatePropertyAsync(UpdatePropertyDto dto)
        {
            try
            {
                var property = await _propertyRepository.GetByIdAsync(dto.PropertyId);
                if (property == null) return null;

                property.PropertyTypeId = dto.PropertyTypeId;
                property.SaleTypeId = dto.SaleTypeId;
                property.Price = dto.Price;
                property.LandSize = dto.LandSize;
                property.Bedrooms = dto.Bedrooms;
                property.Bathrooms = dto.Bathrooms;
                property.Description = dto.Description;

                await _propertyRepository.UpdateAsync(property.PropertyId, property);

                // Update images if provided
                if (dto.ImageUrls != null)
                {
                    // Get current images
                    var currentImages = await _propertyImageRepository
                        .GetAllQuery()
                        .Where(img => img.PropertyId == dto.PropertyId)
                        .ToListAsync();

                    // Delete current images
                    foreach (var img in currentImages)
                    {
                        await _propertyImageRepository.DeleteAsync(img.PropertyImageId);
                    }

                    // Add new images
                    if (dto.ImageUrls.Any())
                    {
                        var images = dto.ImageUrls.Select(url => new PropertyImage
                        {
                            PropertyId = dto.PropertyId,
                            ImageUrl = url
                        }).ToList();

                        await _propertyImageRepository.AddRangeAsync(images);
                    }
                }

                // Update improvements if provided
                if (dto.ImprovementIds != null)
                {
                    // Get current improvements
                    var currentImprovements = await _propertyImprovementRepository
                        .GetAllQuery()
                        .Where(pi => pi.PropertyId == dto.PropertyId)
                        .ToListAsync();

                    // Delete current improvements
                    foreach (var improvement in currentImprovements)
                    {
                        await _propertyImprovementRepository.DeleteAsync(improvement.PropertyImprovementId);
                    }

                    // Add new improvements
                    if (dto.ImprovementIds.Any())
                    {
                        var improvements = dto.ImprovementIds.Select(improvementId => new PropertyImprovement
                        {
                            PropertyId = dto.PropertyId,
                            ImprovementId = improvementId
                        }).ToList();

                        await _propertyImprovementRepository.AddRangeAsync(improvements);
                    }
                }

                return await GetPropertyWithDetailsAsync(dto.PropertyId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<PropertyDto>> FilterPropertiesAsync(int? propertyTypeId, decimal? minPrice, decimal? maxPrice, int? bedrooms, int? bathrooms)
        {
            try
            {
                var query = _propertyRepository
                    .GetAllQueryWithInclude(new List<string> { "PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement" })
                    .Where(p => p.Status == PropertyStatus.Available);

                if (propertyTypeId.HasValue)
                {
                    query = query.Where(p => p.PropertyTypeId == propertyTypeId.Value);
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                if (bedrooms.HasValue)
                {
                    query = query.Where(p => p.Bedrooms == bedrooms.Value);
                }

                if (bathrooms.HasValue)
                {
                    query = query.Where(p => p.Bathrooms == bathrooms.Value);
                }

                var properties = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

                var propertyDtos = new List<PropertyDto>();

                foreach (var property in properties)
                {
                    var agent = await _authService.GetUserById(property.AgentId);
                    var dto = _mapper.Map<PropertyDto>(property);

                    if (agent != null)
                    {
                        dto.AgentName = $"{agent.Name} {agent.LastName}";
                        dto.AgentEmail = agent.Email;
                        dto.AgentPhone = agent.PhoneNumber;
                    }

                    propertyDtos.Add(dto);
                }

                return propertyDtos;
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<bool> MarkPropertyAsSoldAsync(int propertyId)
        {
            try
            {
                var property = await _propertyRepository.GetByIdAsync(propertyId);
                if (property == null) return false;

                property.Status = PropertyStatus.Sold;
                await _propertyRepository.UpdateAsync(propertyId, property);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<int>> GetPropertyImprovementIdsAsync(int propertyId)
        {
            try
            {
                var propertyImprovements = await _propertyImprovementRepository
                    .GetAllQuery()
                    .Where(pi => pi.PropertyId == propertyId)
                    .Select(pi => pi.ImprovementId)
                    .ToListAsync();

                return propertyImprovements;
            }
            catch (Exception)
            {
                return [];
            }
        }

        // Admin Functionalities

        public async Task<int> GetPropertyCountByAgentId(string agentId)
        {
            try
            {
                return await _propertyRepository
                    .GetAllQuery()
                    .CountAsync(p => p.AgentId == agentId);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task DeletePropertiesByAgentId(string agentId)
        {
            try
            {
                var properties = await _propertyRepository
                    .GetAllQuery()
                    .Where(p => p.AgentId == agentId)
                    .ToListAsync();

                foreach (var property in properties)
                {
                    var images = await _propertyImageRepository
                        .GetAllQuery()
                        .Where(img => img.PropertyId == property.PropertyId)
                        .ToListAsync();

                    foreach (var image in images)
                    {
                        await _propertyImageRepository.DeleteAsync(image.PropertyImageId);
                    }

                    var improvements = await _propertyImprovementRepository
                        .GetAllQuery()
                        .Where(pi => pi.PropertyId == property.PropertyId)
                        .ToListAsync();

                    foreach (var improvement in improvements)
                    {
                        await _propertyImprovementRepository.DeleteAsync(improvement.PropertyImprovementId);
                    }

                    await _propertyRepository.DeleteAsync(property.PropertyId);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
