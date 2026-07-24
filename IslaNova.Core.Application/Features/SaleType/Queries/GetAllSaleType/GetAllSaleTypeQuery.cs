using AutoMapper;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.PropertyManagement.SaleType;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.SaleType.Queries.GetAllSaleType
{
    /// <summary>
    /// Query used to retrieve all sale types available in the system.
    /// </summary>
    public class GetAllSaleTypeQuery : IRequest<PaginatedResult<SaleTypeApiDto>>
    {
        public string? Search { get; set; }
        public string? Order { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class GetAllSaleTypeQueryHandler : IRequestHandler<GetAllSaleTypeQuery, PaginatedResult<SaleTypeApiDto>>
    {
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IMapper _mapper;

        public GetAllSaleTypeQueryHandler(ISaleTypeRepository saleTypeRepository, IMapper mapper)
        {
            _saleTypeRepository = saleTypeRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<SaleTypeApiDto>> Handle(GetAllSaleTypeQuery query, CancellationToken cancellationToken)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1) query.Limit = 10;
            if (query.Limit > 100) query.Limit = 100;

            var q = _saleTypeRepository.GetAllQuery();

            // Sort by name
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.ToLower();
                q = q.Where(st => st.Name.ToLower().Contains(s));
            }

            var total = await q.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(total / (double)query.Limit);

            // Sort by name
            q = query.Order?.ToLower() == "asc"
                ? q.OrderBy(st => st.Name)
                : q.OrderByDescending(st => st.Name);

            var items = await q
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<SaleTypeApiDto>
            {
                Data = _mapper.Map<List<SaleTypeApiDto>>(items),
                Meta = new PageMetadata
                {
                    Page = query.Page,
                    Limit = query.Limit,
                    Total = total,
                    TotalPage = totalPages
                }
            };
        }
    }
}
