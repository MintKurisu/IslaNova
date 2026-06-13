using FluentValidation;

namespace IslaNova.Core.Application.Features.Property.Queries.GetPropertiesByAgentId
{
    public class GetPropertiesByAgentIdQueryValidation : AbstractValidator<GetPropertiesByAgentIdQuery>
    {
        public GetPropertiesByAgentIdQueryValidation()
        {
            RuleFor(p => p.AgentId)
                .NotEmpty().WithMessage("Agent ID is required.");
        }
    }
}