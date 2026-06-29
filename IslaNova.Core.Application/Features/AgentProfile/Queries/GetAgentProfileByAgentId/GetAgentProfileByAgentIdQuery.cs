using AutoMapper;
using IslaNova.Core.Application.Dtos.AgentProfile;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.AgentProfile.Queries.GetAgentProfileByAgentId
{
    public class GetAgentProfileByAgentIdQuery : IRequest<AgentProfileDto?>
    {
        [SwaggerParameter(Description = "Agent ID")]
        public string? AgentId { get; set; }
    }

    public class GetAgentProfileByAgentIdQueryHandler : IRequestHandler<GetAgentProfileByAgentIdQuery, AgentProfileDto?>
    {
        private readonly IAgentProfileRepository _agentProfileRepository;
        private readonly IMapper _mapper;

        public GetAgentProfileByAgentIdQueryHandler(
            IAgentProfileRepository agentProfileRepository,
            IMapper mapper)
        {
            _agentProfileRepository = agentProfileRepository;
            _mapper = mapper;
        }

        public async Task<AgentProfileDto?> Handle(GetAgentProfileByAgentIdQuery query, CancellationToken cancellationToken)
        {
            var profile = await _agentProfileRepository.GetByAgentIdAsync(query.AgentId ?? "");
            return _mapper.Map<AgentProfileDto>(profile);
        }
    }
}
