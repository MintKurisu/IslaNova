using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.PropertyManagement.SaleType;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace IslaNova.Core.Application.Features.SaleType.Commands.AddSaleType
{
    /// <summary>
    /// Command used to create a new sale type.
    /// </summary>
    public class AddSaleTypeCommand : IRequest<SaleTypeApiDto>
    {
        /// <example>For Rent</example>
        [SwaggerParameter(Description = "Name of the sale type to create.")]
        public string? Name { get; set; }

        /// <example>Property available for monthly rental.</example>
        [SwaggerParameter(Description = "Describes how the sale type is applied to properties.")]
        public string? Description { get; set; }
    }

    public class AddSaleTypeCommandHandler : IRequestHandler<AddSaleTypeCommand, SaleTypeApiDto>
    {
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IMapper _mapper;

        public AddSaleTypeCommandHandler(ISaleTypeRepository saleTypeRepository, IMapper mapper)
        {
            _saleTypeRepository = saleTypeRepository;
            _mapper = mapper;
        }

        public async Task<SaleTypeApiDto> Handle(AddSaleTypeCommand command, CancellationToken cancellationToken)
        {
            Domain.Entities.PropertyManagement.SaleType entity = new()
            {
                Name = command.Name ?? "",
                Description = command.Description ?? "",
            };

            var response = await _saleTypeRepository.AddAsync(entity);

            if (response == null)
                throw new ApiException("Error creating sale type", (int)HttpStatusCode.BadRequest);

            return _mapper.Map<SaleTypeApiDto>(response);
        }
    }
}
