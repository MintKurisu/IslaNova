using FluentValidation;

namespace IslaNova.Core.Application.Features.Improvement.Commands.UpdateImprovement
{
    public class UpdateImprovementCommandValidation : AbstractValidator<UpdateImprovementCommand>
    {
        public UpdateImprovementCommandValidation()
        {
            RuleFor(i => i.ImprovementId)
                  .NotNull().WithMessage("Improvement ID is required.")
                  .GreaterThan(0).WithMessage("Improvement ID must be greater than 0.")
                  .WithMessage("Improvement ID is required.");

            RuleFor(i => i.Name)
                 .NotEmpty().WithMessage("Improvement name is required.")
                 .MaximumLength(100).WithMessage("Improvement name must not exceed 100 characters.");

            RuleFor(i => i.Description)
                 .NotEmpty().WithMessage("Improvement description is required.")
                 .MaximumLength(500).WithMessage("Improvement description must not exceed 500 characters.");
        }
    }
}
