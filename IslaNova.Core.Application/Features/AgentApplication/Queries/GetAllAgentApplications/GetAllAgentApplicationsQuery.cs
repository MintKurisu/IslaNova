using AutoMapper;
using IslaNova.Core.Application.Dtos.AgentApplication;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IslaNova.Core.Application.Features.AgentApplication.Queries.GetAllAgentApplications
{
    public class GetAllAgentApplicationsQuery : IRequest<IList<AgentApplicationDto>> { }

    public class GetAllAgentApplicationsQueryHandler : IRequestHandler<GetAllAgentApplicationsQuery, IList<AgentApplicationDto>>
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

        public async Task<IList<AgentApplicationDto>> Handle(GetAllAgentApplicationsQuery query, CancellationToken cancellationToken)
        {
            var applications = await _repository
                .GetAllQuery()
                .OrderByDescending(a => a.SubmittedAt)
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

            return dtos;
        }
    }
}
