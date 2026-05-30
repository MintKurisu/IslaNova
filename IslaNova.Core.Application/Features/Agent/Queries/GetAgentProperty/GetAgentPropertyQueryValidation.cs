using FluentValidation;

namespace IslaNova.Core.Application.Features.Agent.Queries.GetAgentProperty
{
    public class GetAgentPropertyQueryValidation : AbstractValidator<GetAgentPropertyQuery>
    {
        public GetAgentPropertyQueryValidation()
        {
            RuleFor(p => p.Id)
             .NotEmpty().WithMessage("ID is required.");
        }
    }
}
