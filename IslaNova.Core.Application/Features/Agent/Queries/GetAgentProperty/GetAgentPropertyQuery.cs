using AutoMapper;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Agent.Queries.GetAgentProperty
{
    /// <summary>
    /// Query used to retrieve all properties of an agente for it's unique identifier
    /// </summary>
    /// 
    public class GetAgentPropertyQuery : IRequest<PaginatedResult<PropertyDto>>
    {
        public string? Id { get; set; }
        public string? Search { get; set; }
        public string? Order { get; set; } = "desc";
        public string? SortBy { get; set; } = "createdAt"; // "createdAt" | "price"
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class GetAgentPropertyQueryHandler : IRequestHandler<GetAgentPropertyQuery, PaginatedResult<PropertyDto>>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public GetAgentPropertyQueryHandler(
            IPropertyRepository propertyRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<PropertyDto>> Handle(GetAgentPropertyQuery query, CancellationToken cancellationToken)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1) query.Limit = 10;
            if (query.Limit > 100) query.Limit = 100;

            var q = _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement"])
                .Where(p => p.AgentId == query.Id);

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

            var properties = await q
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            var agent = await _authService.GetUserById(query.Id ?? "");
            var dtoList = new List<PropertyDto>();

            foreach (var property in properties)
            {
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