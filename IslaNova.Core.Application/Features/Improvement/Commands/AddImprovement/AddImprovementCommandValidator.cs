using FluentValidation;

namespace IslaNova.Core.Application.Features.Improvement.Commands.AddImprovement
{
    public class AddImprovementCommandValidator : AbstractValidator<AddImprovementCommand>
    {
        public AddImprovementCommandValidator()
        {
            RuleFor(i => i.Name)
                .NotEmpty().WithMessage("Improvement name is required.")
                .MaximumLength(100).WithMessage("Improvement name must not exceed 100 characters.");

            RuleFor(i => i.Description)
              .NotEmpty().WithMessage("Improvement description is required.")
              .MaximumLength(500).WithMessage("Improvement description must not exceed 500 characters.");
        }
    }
}
