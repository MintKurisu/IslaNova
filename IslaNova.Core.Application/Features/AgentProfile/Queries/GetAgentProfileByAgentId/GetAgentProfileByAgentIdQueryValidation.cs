using FluentValidation;
using IslaNova.Core.Application.Interfaces.Auth;

namespace IslaNova.Core.Application.Features.AgentProfile.Queries.GetAgentProfileByAgentId
{
    public class GetAgentProfileByAgentIdQueryValidation : AbstractValidator<GetAgentProfileByAgentIdQuery>
    {
        private readonly IAuthServiceForWebApi _authService;

        public GetAgentProfileByAgentIdQueryValidation(IAuthServiceForWebApi authService)
        {
            _authService = authService;

            RuleFor(p => p.AgentId)
                .NotEmpty().WithMessage("Agent ID is required.")
                .MustAsync(ExistInIdentity).WithMessage("The specified Agent ID does not exist.");
        }

        private async Task<bool> ExistInIdentity(string? agentId, CancellationToken cancellationToken)
        {
            var user = await _authService.GetUserById(agentId ?? "");
            return user != null;
        }
    }
}
