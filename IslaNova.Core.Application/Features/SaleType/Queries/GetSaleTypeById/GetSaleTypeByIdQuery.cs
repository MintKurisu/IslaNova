using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.PropertyManagement.SaleType;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.SaleType.Queries.GetSaleTypeById
{
    /// <summary>
    /// Query used to retrieve a single sale type by it's unique Code
    /// </summary>
    public class GetSaleTypeByIdQuery : IRequest<SaleTypeApiDto?>
    {
        /// <example>23</example>
        [SwaggerParameter(Description = "Unique identifier of the sale type.")]
        public int SaleTypeId { get; set; }
    }

    public class GetSaleTypeByIdQueryHandler : IRequestHandler<GetSaleTypeByIdQuery, SaleTypeApiDto?>
    {

        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IMapper _mapper;

        public GetSaleTypeByIdQueryHandler(ISaleTypeRepository saleTypeRepository, IMapper mapper)
        {
            _saleTypeRepository = saleTypeRepository;
            _mapper = mapper;
        }

        public async Task<SaleTypeApiDto?> Handle(GetSaleTypeByIdQuery query, CancellationToken cancellationToken)
        {
            var saleType = await _saleTypeRepository.GetByIdAsync(query.SaleTypeId);

            if (saleType == null)
                return null;

            return _mapper.Map<SaleTypeApiDto>(saleType);
        }
    }
}
