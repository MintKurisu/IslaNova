using AutoMapper;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.PropertyManagement.PropertyType;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.PropertyType.Queries.GetAllPropertyType
{
    /// <summary>
    /// Query used to retrieve all property types available in the system.
    /// </summary>
    public class GetAllPropertyTypeQuery : IRequest<PaginatedResult<PropertyTypeApiDto>>
    {
        public string? Search { get; set; }
        public string? Order { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class GetAllPropertyTypeQueryHandler : IRequestHandler<GetAllPropertyTypeQuery, PaginatedResult<PropertyTypeApiDto>>
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IMapper _mapper;

        public GetAllPropertyTypeQueryHandler(IPropertyTypeRepository propertyTypeRepository, IMapper mapper)
        {
            _propertyTypeRepository = propertyTypeRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<PropertyTypeApiDto>> Handle(GetAllPropertyTypeQuery query, CancellationToken cancellationToken)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1) query.Limit = 10;
            if (query.Limit > 100) query.Limit = 100;

            var q = _propertyTypeRepository.GetAllQuery();

            // Search by name
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.ToLower();
                q = q.Where(pt => pt.Name.ToLower().Contains(s));
            }

            var total = await q.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(total / (double)query.Limit);

            // Search by name
            q = query.Order?.ToLower() == "asc"
                ? q.OrderBy(pt => pt.Name)
                : q.OrderByDescending(pt => pt.Name);

            var items = await q
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<PropertyTypeApiDto>
            {
                Data = _mapper.Map<List<PropertyTypeApiDto>>(items),
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
