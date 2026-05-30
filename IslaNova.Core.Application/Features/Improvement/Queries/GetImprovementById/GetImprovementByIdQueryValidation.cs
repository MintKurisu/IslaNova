using FluentValidation;

namespace IslaNova.Core.Application.Features.Improvement.Queries.GetImprovementById
{
    internal class GetImprovementByIdQueryValidation : AbstractValidator<GetImprovementByIdQuery>
    {
        public GetImprovementByIdQueryValidation()
        {
            RuleFor(i => i.ImprovementId)
              .NotNull().WithMessage("Improvement ID is required.")
              .GreaterThan(0).WithMessage("Improvement ID must be greater than 0.")
              .WithMessage("Improvement ID is required.");
        }
    }
}
