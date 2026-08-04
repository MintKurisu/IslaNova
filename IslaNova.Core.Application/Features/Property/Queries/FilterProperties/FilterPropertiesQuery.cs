using AutoMapper;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Property.Queries.FilterProperties
{
    public class FilterPropertiesQuery : IRequest<PaginatedResult<PropertyDto>>
    {
        public int? PropertyTypeId { get; set; }
        public int? SaleTypeId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }

        // Additonal filters for search, order, sortBy, page, and limit
        public string? Search { get; set; }
        public string? Order { get; set; } = "desc";
        public string? SortBy { get; set; } = "createdAt";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class FilterPropertiesQueryHandler : IRequestHandler<FilterPropertiesQuery, PaginatedResult<PropertyDto>>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public FilterPropertiesQueryHandler(
            IPropertyRepository propertyRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<PropertyDto>> Handle(FilterPropertiesQuery query, CancellationToken cancellationToken)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1) query.Limit = 10;
            if (query.Limit > 100) query.Limit = 100;

            var q = _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement"])
                .Where(p => p.Status == PropertyStatus.Available);

            // Filtros
            if (query.PropertyTypeId.HasValue)
                q = q.Where(p => p.PropertyTypeId == query.PropertyTypeId.Value);

            if (query.SaleTypeId.HasValue)
                q = q.Where(p => p.SaleTypeId == query.SaleTypeId.Value);

            if (query.MinPrice.HasValue)
                q = q.Where(p => p.Price >= query.MinPrice.Value);

            if (query.MaxPrice.HasValue)
                q = q.Where(p => p.Price <= query.MaxPrice.Value);

            if (query.Bedrooms.HasValue)
                q = q.Where(p => p.Bedrooms == query.Bedrooms.Value);

            if (query.Bathrooms.HasValue)
                q = q.Where(p => p.Bathrooms == query.Bathrooms.Value);

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.ToLower();
                q = q.Where(p =>
                    p.Code.ToLower().Contains(s) ||
                    p.Description.ToLower().Contains(s) ||
                    (p.Address != null && p.Address.ToLower().Contains(s)) ||
                    (p.City != null && p.City.ToLower().Contains(s)));
            }

            var total = await q.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(total / (double)query.Limit);

            // Sort
            q = (query.SortBy?.ToLower(), query.Order?.ToLower()) switch
            {
                ("price", "asc") => q.OrderBy(p => p.Price),
                ("price", "desc") => q.OrderByDescending(p => p.Price),
                (_, "asc") => q.OrderBy(p => p.CreatedAt),
                _ => q.OrderByDescending(p => p.CreatedAt)
            };

            var properties = await q
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            var dtoList = new List<PropertyDto>();
            foreach (var property in properties)
            {
                var agent = await _authService.GetUserById(property.AgentId);
                var dto = _mapper.Map<PropertyDto>(property);
                if (agent != null)
                {
                    dto.AgentName = $"{agent.Name} {agent.LastName}";
                    dto.AgentEmail = agent.Email;
                    dto.AgentPhone = agent.PhoneNumber;
                    dto.AgentProfileImage = agent.ProfileImage;
                }
                dtoList.Add(dto);
            }

            return new PaginatedResult<PropertyDto>
            {
                Data = dtoList,
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