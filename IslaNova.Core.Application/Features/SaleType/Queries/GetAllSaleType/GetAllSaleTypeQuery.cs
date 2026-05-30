using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.PropertyManagement.SaleType;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;

namespace IslaNova.Core.Application.Features.SaleType.Queries.GetAllSaleType
{
    /// <summary>
    /// Query used to retrieve all sale types available in the system.
    /// </summary>
    public class GetAllSaleTypeQuery : IRequest<IList<SaleTypeApiDto>> { }

    public class GetAllSaleTypeQueryHandler : IRequestHandler<GetAllSaleTypeQuery, IList<SaleTypeApiDto>>
    {

        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IMapper _mapper;

        public GetAllSaleTypeQueryHandler(ISaleTypeRepository saleTypeRepository, IMapper mapper)
        {
            _saleTypeRepository = saleTypeRepository;
            _mapper = mapper;
        }

        public async Task<IList<SaleTypeApiDto>> Handle(GetAllSaleTypeQuery query, CancellationToken cancellationToken)
        {
            var propertyTypeList = await _saleTypeRepository.GetAllListAsync();

            return _mapper.Map<IList<SaleTypeApiDto>>(propertyTypeList);
        }
    }
}
