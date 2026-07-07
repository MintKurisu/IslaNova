using IslaNova.Core.Domain.Interfaces.AccountManagement;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.AgentProfile.Commands.DeleteAgentProfile
{
    public class DeleteAgentProfileCommand : IRequest<bool>
    {
        [SwaggerParameter(Description = "Agent ID whose profile will be deleted")]
        public string? AgentId { get; set; }
    }

    public class DeleteAgentProfileCommandHandler : IRequestHandler<DeleteAgentProfileCommand, bool>
    {
        private readonly IAgentProfileRepository _agentProfileRepository;

        public DeleteAgentProfileCommandHandler(IAgentProfileRepository agentProfileRepository)
        {
            _agentProfileRepository = agentProfileRepository;
        }

        public async Task<bool> Handle(DeleteAgentProfileCommand command, CancellationToken cancellationToken)
        {
            var profile = await _agentProfileRepository.GetByAgentIdAsync(command.AgentId ?? "");
            if (profile == null) return false;

            await _agentProfileRepository.DeleteAsync(profile.AgentProfileId);
            return true;
        }
    }
}
