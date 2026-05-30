using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.PropertyManagement.SaleType;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace IslaNova.Core.Application.Features.SaleType.Commands.UpdateSaleType
{
    /// <summary>
    /// Command used to update an existing sale type.
    /// </summary>
    public class UpdateSaleTypeCommand : IRequest<SaleTypeApiDto>
    {
        /// <example>23</example>
        [SwaggerParameter(Description = "Unique identifier of the sale type.")]
        [SwaggerSchema(ReadOnly = true)]
        public int SaleTypeId { get; set; }

        /// <example>For Rent</example>
        [SwaggerParameter(Description = "Name of the sale type to create.")]
        public string? Name { get; set; }

        /// <example>Property available for monthly rental.</example>
        [SwaggerParameter(Description = "Describes how the sale type is applied to properties.")]
        public string? Description { get; set; }
    }

    public class UpdateSaleTypeCommandHandler : IRequestHandler<UpdateSaleTypeCommand, SaleTypeApiDto>
    {
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IMapper _mapper;

        public UpdateSaleTypeCommandHandler(ISaleTypeRepository saleTypeRepository, IMapper mapper)
        {
            _saleTypeRepository = saleTypeRepository;
            _mapper = mapper;
        }

        public async Task<SaleTypeApiDto> Handle(UpdateSaleTypeCommand command, CancellationToken cancellationToken)
        {
            var entity = await _saleTypeRepository.GetByIdAsync(command.SaleTypeId);

            if (entity == null)
                throw new ApiException("Sale Type not found", (int)HttpStatusCode.BadRequest);


            Domain.Entities.PropertyManagement.SaleType newEntity = new()
            {
                SaleTypeId = command.SaleTypeId,
                Name = command.Name ?? "",
                Description = command.Description ?? "",
            };

            var response = await _saleTypeRepository.UpdateAsync(command.SaleTypeId, newEntity);

            if (response == null)
                throw new ApiException("Error updating sale type", (int)HttpStatusCode.InternalServerError);

            return _mapper.Map<SaleTypeApiDto>(response);
        }
    }
}
