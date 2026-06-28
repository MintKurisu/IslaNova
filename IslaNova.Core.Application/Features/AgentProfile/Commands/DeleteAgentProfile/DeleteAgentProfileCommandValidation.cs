using FluentValidation;
using IslaNova.Core.Domain.Interfaces.AccountManagement;

namespace IslaNova.Core.Application.Features.AgentProfile.Commands.DeleteAgentProfile
{
    public class DeleteAgentProfileCommandValidation : AbstractValidator<DeleteAgentProfileCommand>
    {
        private readonly IAgentProfileRepository _agentProfileRepository;

        public DeleteAgentProfileCommandValidation(IAgentProfileRepository agentProfileRepository)
        {
            _agentProfileRepository = agentProfileRepository;

            RuleFor(p => p.AgentId)
                .NotEmpty().WithMessage("Agent ID is required.")
                .MustAsync(HaveProfile).WithMessage("No profile found for this agent.");
        }

        private async Task<bool> HaveProfile(string? agentId, CancellationToken cancellationToken)
        {
            var profile = await _agentProfileRepository.GetByAgentIdAsync(agentId ?? "");
            return profile != null;
        }
    }
}
