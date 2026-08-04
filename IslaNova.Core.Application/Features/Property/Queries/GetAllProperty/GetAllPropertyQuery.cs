using AutoMapper;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Property.Queries.GetAllProperty
{
    /// <summary>
    /// Query used to retrieve all properties available in the system.
    /// </summary>
    public class GetAllPropertyQuery : IRequest<PaginatedResult<PropertyDto>>
    {
        public string? Search { get; set; }
        public string? Order { get; set; } = "desc";
        public string? SortBy { get; set; } = "createdAt";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class GetAllPropertyQueryHandler : IRequestHandler<GetAllPropertyQuery, PaginatedResult<PropertyDto>>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAuthServiceForWebApi _authServiceForWebApi;
        private readonly IMapper _mapper;

        public GetAllPropertyQueryHandler(
            IPropertyRepository propertyRepository,
            IAuthServiceForWebApi authServiceForWebApi,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _authServiceForWebApi = authServiceForWebApi;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<PropertyDto>> Handle(GetAllPropertyQuery query, CancellationToken cancellationToken)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1) query.Limit = 10;
            if (query.Limit > 100) query.Limit = 100;

            var q = _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement"]);

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.ToLower();
                q = q.Where(p =>
                    p.Code.ToLower().Contains(s) ||
                    p.Description.ToLower().Contains(s));
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

            var entities = await q
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            var dtoList = new List<PropertyDto>();
            foreach (var entity in entities)
            {
                var agent = await _authServiceForWebApi.GetUserById(entity.AgentId);
                var dto = _mapper.Map<PropertyDto>(entity);
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
