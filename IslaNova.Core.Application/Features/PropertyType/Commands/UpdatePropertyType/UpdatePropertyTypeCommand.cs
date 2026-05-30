using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.PropertyManagement.PropertyType;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace IslaNova.Core.Application.Features.PropertyType.Commands.UpdatePropertyType
{
    /// <summary>
    /// Command used to update an existing property type.
    /// </summary>
    public class UpdatePropertyTypeCommand : IRequest<PropertyTypeApiDto?>
    {
        /// <example>23</example>
        [SwaggerParameter(Description = "Unique identifier of the property type.")]
        [SwaggerSchema(ReadOnly = true)]
        public int PropertyTypeId { get; set; }

        /// <example>Apartment</example>
        [SwaggerParameter(Description = "Name of the property type to create.")]
        public string? Name { get; set; }

        /// <example>Residential multi-unit building suitable for families or individuals.</example>
        [SwaggerParameter(Description = "A brief description of what this property type represents.")]
        public string? Description { get; set; }
    }

    public class UpdatePropertyTypeCommandHandler : IRequestHandler<UpdatePropertyTypeCommand, PropertyTypeApiDto?>
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IMapper _mapper;

        public UpdatePropertyTypeCommandHandler(IPropertyTypeRepository propertyTypeRepository, IMapper mapper)
        {
            _propertyTypeRepository = propertyTypeRepository;
            _mapper = mapper;
        }

        public async Task<PropertyTypeApiDto?> Handle(UpdatePropertyTypeCommand command, CancellationToken cancellationToken)
        {
            var entity = await _propertyTypeRepository.GetByIdAsync(command.PropertyTypeId);

            if (entity == null)
                throw new ApiException("Property Type not found", (int)HttpStatusCode.BadRequest);

            Domain.Entities.PropertyManagement.PropertyType newEntity = new()
            {
                PropertyTypeId = command.PropertyTypeId,
                Name = command.Name ?? "",
                Description = command.Description ?? "",
            };

            var response = await _propertyTypeRepository.UpdateAsync(command.PropertyTypeId, newEntity);

            if (response == null)
                throw new ApiException("Error updating property type", (int)HttpStatusCode.InternalServerError);

            return _mapper.Map<PropertyTypeApiDto>(response);
        }
    }
}
