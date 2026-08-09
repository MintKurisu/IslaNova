using AutoMapper;
using IslaNova.Core.Application.Dtos.AgentApplication;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.AgentApplication.Queries.GetAgentApplicationById
{
    public class GetAgentApplicationByIdQuery : IRequest<AgentApplicationDto?>
    {
        [SwaggerParameter(Description = "Application ID")]
        public int ApplicationId { get; set; }
    }

    public class GetAgentApplicationByIdQueryHandler : IRequestHandler<GetAgentApplicationByIdQuery, AgentApplicationDto?>
    {
        private readonly IAgentApplicationRepository _repository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public GetAgentApplicationByIdQueryHandler(
            IAgentApplicationRepository repository,
            IAuthServiceForWebApi authService,
            IMapper mapper)
        {
            _repository = repository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<AgentApplicationDto?> Handle(GetAgentApplicationByIdQuery query, CancellationToken cancellationToken)
        {
            var application = await _repository.GetByIdAsync(query.ApplicationId);
            if (application == null) return null;

            var dto = _mapper.Map<AgentApplicationDto>(application);
            var agent = await _authService.GetUserById(application.UserId);
            if (agent != null)
            {
                dto.AgentName = $"{agent.Name} {agent.LastName}";
                dto.AgentEmail = agent.Email;
                dto.AgentPhone = agent.PhoneNumber;
                dto.AgentProfileImage = agent.ProfileImage;
            }

            return dto;
        }
    }
}
