using MediatR;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace IslaNova.Core.Application.Features.SaleType.Commands.DeleteSaleType
{
    /// <summary>
    /// Command used to delete a single sale type by it's unique ID
    /// </summary>
    public class DeleteSaleTypeCommand : IRequest<Unit>
    {
        /// <example>23</example>
        [SwaggerParameter(Description = "Unique identifier of the sale type.")]
        public int SaleTypeId { get; set; }
    }
    public class DeleteSaleTypeCommandHandler : IRequestHandler<DeleteSaleTypeCommand, Unit>
    {
        private readonly ISaleTypeRepository _saleTypeRepository;

        public DeleteSaleTypeCommandHandler(ISaleTypeRepository saleTypeRepository)
        {
            _saleTypeRepository = saleTypeRepository;
        }

        public async Task<Unit> Handle(DeleteSaleTypeCommand command, CancellationToken cancellationToken)
        {
            var entity = await _saleTypeRepository.GetByIdAsync(command.SaleTypeId);

            if (entity == null)
                throw new ApiException("Error deleting sale type", (int)HttpStatusCode.InternalServerError);

            await _saleTypeRepository.DeleteAsync(command.SaleTypeId);
            return Unit.Value;
        }
    }
}
