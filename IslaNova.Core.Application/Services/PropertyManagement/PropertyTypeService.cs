using AutoMapper;
using IslaNova.Core.Application.Dtos.PropertyManagement.PropertyType;
using IslaNova.Core.Application.Interfaces.PropertyManagement;
using IslaNova.Core.Application.Services.Base;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;

namespace IslaNova.Core.Application.Services.PropertyManagement
{
    public class PropertyTypeService : GenericService<PropertyType, PropertyTypeDto>, IPropertyTypeService
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IMapper _mapper;

        public PropertyTypeService(IPropertyTypeRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
            _propertyTypeRepository = repository;
            _mapper = mapper;
        }

        public override async Task<List<PropertyTypeDto>> GetAllAsync()
        {
            try
            {
                var propertyTypes = await _propertyTypeRepository.GetAllListWithIncludeAsync(
                    new List<string> { "Properties" }
                );

                var propertyTypeDtos = _mapper.Map<List<PropertyTypeDto>>(propertyTypes);
                return propertyTypeDtos;
            }
            catch (Exception)
            {
                return new List<PropertyTypeDto>();
            }
        }

        public override async Task<PropertyTypeDto?> GetByIdAsync(int id)
        {
            try
            {
                var propertyType = await _propertyTypeRepository.GetByIdWithIncludeAsync(
                    id,
                    new List<string> { "Properties" }
                );

                if (propertyType == null)
                {
                    return null;
                }

                var propertyTypeDto = _mapper.Map<PropertyTypeDto>(propertyType);

                return propertyTypeDto;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
