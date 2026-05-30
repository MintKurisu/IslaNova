using FluentValidation;

namespace IslaNova.Core.Application.Features.Agent.Commands.ChangeAgentStatus
{
    public class ChangeAgentStatusCommandValidation : AbstractValidator<ChangeAgentStatusCommand>
    {
        public ChangeAgentStatusCommandValidation()
        {
            RuleFor(p => p.Id)
              .NotEmpty().WithMessage("ID is required.");
        }
    }
}
