using AutoMapper;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.AgentApplication;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Enums;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.AgentApplication.Queries.GetAllAgentApplications
{
    public class GetAllAgentApplicationsQuery : IRequest<PaginatedResult<AgentApplicationDto>>
    {
        public string? Search { get; set; }
        public string? Order { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    public class GetAllAgentApplicationsQueryHandler : IRequestHandler<GetAllAgentApplicationsQuery, PaginatedResult<AgentApplicationDto>>
    {
        private readonly IAgentApplicationRepository _repository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public GetAllAgentApplicationsQueryHandler(
            IAgentApplicationRepository repository,
            IAuthServiceForWebApi authService,
            IMapper mapper)
        {
            _repository = repository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<AgentApplicationDto>> Handle(GetAllAgentApplicationsQuery query, CancellationToken cancellationToken)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.Limit < 1) query.Limit = 10;
            if (query.Limit > 100) query.Limit = 100;

            var q = _repository.GetAllQuery();

            // Search for AgencyName, EmploymentType, or Status
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.ToLower();
                var statusFilter = Enum.TryParse<ApplicationStatus>(query.Search, true, out var parsed) ? parsed : (ApplicationStatus?)null;

                q = q.Where(a =>
                    (a.AgencyName != null && a.AgencyName.ToLower().Contains(s)) ||
                    (statusFilter.HasValue && a.Status == statusFilter.Value));
            }

            var total = await q.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(total / (double)query.Limit);

            // Sort SubmittedAt
            q = query.Order?.ToLower() == "asc"
                ? q.OrderBy(a => a.SubmittedAt)
                : q.OrderByDescending(a => a.SubmittedAt);

            var applications = await q
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            var dtos = new List<AgentApplicationDto>();
            foreach (var application in applications)
            {
                var dto = _mapper.Map<AgentApplicationDto>(application);
                var agent = await _authService.GetUserById(application.UserId);
                if (agent != null)
                {
                    dto.AgentName = $"{agent.Name} {agent.LastName}";
                    dto.AgentEmail = agent.Email;
                    dto.AgentPhone = agent.PhoneNumber;
                }
                dtos.Add(dto);
            }

            return new PaginatedResult<AgentApplicationDto>
            {
                Data = dtos,
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
