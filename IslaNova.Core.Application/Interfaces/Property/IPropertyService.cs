using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Base;

namespace IslaNova.Core.Application.Interfaces.Property
{
    public interface IPropertyService : IGenericService<PropertyDto>
    {
        Task<List<PropertyDto>> GetAvailablePropertiesAsync();
        Task<List<PropertyDto>> GetPropertiesByAgentIdAsync(string agentId);
        Task<PropertyDto?> GetPropertyByCodeAsync(string code);
        Task<PropertyDto?> GetPropertyWithDetailsAsync(int id);
        Task<PropertyDto?> CreatePropertyAsync(CreatePropertyDto dto);
        Task<PropertyDto?> UpdatePropertyAsync(UpdatePropertyDto dto);
        Task<List<PropertyDto>> FilterPropertiesAsync(int? propertyTypeId, decimal? minPrice, decimal? maxPrice, int? bedrooms, int? bathrooms);
        Task<bool> MarkPropertyAsSoldAsync(int propertyId);
        Task<List<int>> GetPropertyImprovementIdsAsync(int propertyId);

        // Admin Functionalities

        Task<int> GetPropertyCountByAgentId(string agentId);
        Task DeletePropertiesByAgentId(string agentId);
    }
}
