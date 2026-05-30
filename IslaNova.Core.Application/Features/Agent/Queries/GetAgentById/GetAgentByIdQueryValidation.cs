using FluentValidation;
using IslaNova.Core.Application.Features.Agent.Queries.GetById;

namespace IslaNova.Core.Application.Features.Agent.Queries.GetAgentById
{
    public class GetAgentByIdQueryValidation : AbstractValidator<GetAgentByIdQuery>
    {
        public GetAgentByIdQueryValidation()
        {
            RuleFor(p => p.Id)
            .NotEmpty().WithMessage("ID is required.");
        }
    }
}
