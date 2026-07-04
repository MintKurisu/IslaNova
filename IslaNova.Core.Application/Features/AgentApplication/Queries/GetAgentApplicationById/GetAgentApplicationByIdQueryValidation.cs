using FluentValidation;

namespace IslaNova.Core.Application.Features.AgentApplication.Queries.GetAgentApplicationById
{
    public class GetAgentApplicationByIdQueryValidation : AbstractValidator<GetAgentApplicationByIdQuery>
    {
        public GetAgentApplicationByIdQueryValidation()
        {
            RuleFor(x => x.ApplicationId)
                .GreaterThan(0).WithMessage("Application ID must be greater than 0.");
        }
    }
}
