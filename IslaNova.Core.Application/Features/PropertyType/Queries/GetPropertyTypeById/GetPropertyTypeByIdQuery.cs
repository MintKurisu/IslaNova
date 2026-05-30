using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.PropertyManagement.PropertyType;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.PropertyType.Queries.GetPropertyTypeById
{
    /// <summary>
    /// Query used to retrieve a single property type by it's unique Code
    /// </summary>
    public class GetPropertyTypeByIdQuery : IRequest<PropertyTypeApiDto?>
    {
        /// <example>23</example>
        [SwaggerParameter(Description = "Unique identifier of the property type.")]
        public int PropertyTypeId { get; set; }
    }

    public class GetPropertyTypeByIdQueryHandler : IRequestHandler<GetPropertyTypeByIdQuery, PropertyTypeApiDto?>
    {

        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IMapper _mapper;

        public GetPropertyTypeByIdQueryHandler(IPropertyTypeRepository propertyTypeRepository, IMapper mapper)
        {
            _propertyTypeRepository = propertyTypeRepository;
            _mapper = mapper;
        }

        public async Task<PropertyTypeApiDto?> Handle(GetPropertyTypeByIdQuery query, CancellationToken cancellationToken)
        {
            var propertyType = await _propertyTypeRepository.GetByIdAsync(query.PropertyTypeId);

            if (propertyType == null)
            {
                return null;
            }

            return _mapper.Map<PropertyTypeApiDto>(propertyType);
        }
    }
}
