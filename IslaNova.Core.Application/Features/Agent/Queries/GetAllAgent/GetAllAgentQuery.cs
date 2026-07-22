using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Agent.Queries.GetAllAgent
{
    /// <summary>
    /// Query used to retrieve all agents available in the system.
    /// </summary>
    public class GetAllAgentQuery : IRequest<PaginatedResult<AgentUserDto>>
    {
        public string? Search { get; set; }
        public string? Order { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class GetAllAgentQueryHandler : IRequestHandler<GetAllAgentQuery, PaginatedResult<AgentUserDto>>
    {
        private readonly IAuthServiceForWebApi _authServiceForWebApi;
        private readonly IPropertyRepository _propertyRepository;

        public GetAllAgentQueryHandler(IAuthServiceForWebApi authServiceForWebApi, IPropertyRepository propertyRepository)
        {
            _authServiceForWebApi = authServiceForWebApi;
            _propertyRepository = propertyRepository;
        }

        public async Task<PaginatedResult<AgentUserDto>> Handle(GetAllAgentQuery query, CancellationToken cancellationToken)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1) query.Limit = 10;
            if (query.Limit > 100) query.Limit = 100;

            var entityList = await _authServiceForWebApi.GetAllUserByRole(Roles.Agent);

            // In memory search
            IEnumerable<UserDto> filtered = entityList;

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.ToLower();
                filtered = filtered.Where(u =>
                    u.Name.ToLower().Contains(s) ||
                    u.LastName.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s));
            }

            // Sort
            filtered = query.Order?.ToLower() == "asc"
                ? filtered.OrderBy(u => u.Name)
                : filtered.OrderByDescending(u => u.Name);

            var total = filtered.Count();
            var totalPages = (int)Math.Ceiling(total / (double)query.Limit);

            // in memory pagination
            var paged = filtered
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToList();

            // map to dto and get property count for each agent
            var dtoList = new List<AgentUserDto>();
            foreach (var entity in paged)
            {
                var propertyCount = await _propertyRepository
                    .GetAllQuery()
                    .Where(p => p.AgentId == entity.Id)
                    .CountAsync(cancellationToken);

                dtoList.Add(new AgentUserDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    LastName = entity.LastName,
                    Email = entity.Email,
                    PhoneNumber = entity.PhoneNumber,
                    PropertyCount = propertyCount
                });
            }

            return new PaginatedResult<AgentUserDto>
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
