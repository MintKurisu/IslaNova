using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.PropertyManagement.PropertyType;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace IslaNova.Core.Application.Features.PropertyType.Commands.AddPropertyType
{
    /// <summary>
    /// Command used to create a new property type.
    /// </summary>
    public class AddPropertyTypeCommand : IRequest<PropertyTypeApiDto?>
    {
        /// <example>Apartment</example>
        [SwaggerParameter(Description = "Name of the property type to create.")]
        public string? Name { get; set; }

        /// <example>Residential building suitable for families or individuals.</example>
        [SwaggerParameter(Description = "A brief description of what this property type represents.")]
        public string? Description { get; set; }

        public class AddPropertyTypeCommandHandler : IRequestHandler<AddPropertyTypeCommand, PropertyTypeApiDto?>
        {
            private readonly IPropertyTypeRepository _propertyTypeRepository;
            private readonly IMapper _mapper;

            public AddPropertyTypeCommandHandler(IPropertyTypeRepository propertyTypeRepository, IMapper mapper)
            {
                _propertyTypeRepository = propertyTypeRepository;
                _mapper = mapper;
            }

            public async Task<PropertyTypeApiDto?> Handle(AddPropertyTypeCommand command, CancellationToken cancellationToken)
            {
                Domain.Entities.PropertyManagement.PropertyType entity = new()
                {
                    Name = command.Name ?? "",
                    Description = command.Description ?? "",
                };

                var response = await _propertyTypeRepository.AddAsync(entity);

                if (response == null)
                    throw new ApiException("Error creating property type", (int)HttpStatusCode.BadRequest);

                return _mapper.Map<PropertyTypeApiDto>(response);
            }
        }

    }
}
