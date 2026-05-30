using MediatR;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace IslaNova.Core.Application.Features.PropertyType.Commands.DeletePropertyType
{
    /// <summary>
    /// Command used to delete a single property type by it's unique Code
    /// </summary>
    public class DeletePropertyTypeCommand : IRequest<Unit>
    {
        /// <example>23</example>
        [SwaggerParameter(Description = "Unique identifier of the property type.")]
        public int PropertyTypeId { get; set; }
    }

    public class DeletePropertyTypeCommandHandler : IRequestHandler<DeletePropertyTypeCommand, Unit>
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;

        public DeletePropertyTypeCommandHandler(IPropertyTypeRepository propertyTypeRepository)
        {
            _propertyTypeRepository = propertyTypeRepository;
        }

        public async Task<Unit> Handle(DeletePropertyTypeCommand command, CancellationToken cancellationToken)
        {

            var entity = await _propertyTypeRepository.GetByIdAsync(command.PropertyTypeId);

            if (entity == null)
                throw new ApiException("Error deleting property type", (int)HttpStatusCode.InternalServerError);

            await _propertyTypeRepository.DeleteAsync(command.PropertyTypeId);
            return Unit.Value;
        }
    }
}
