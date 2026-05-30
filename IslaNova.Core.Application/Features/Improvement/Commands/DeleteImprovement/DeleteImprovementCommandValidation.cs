using FluentValidation;

namespace IslaNova.Core.Application.Features.Improvement.Commands.DeleteImprovement
{
    public class DeleteImprovementCommandValidation : AbstractValidator<DeleteImprovementCommand>
    {
        public DeleteImprovementCommandValidation()
        {
            RuleFor(i => i.ImprovementId)
               .NotNull().WithMessage("Improvement ID is required.")
               .GreaterThan(0).WithMessage("Improvement ID must be greater than 0.")
               .WithMessage("Improvement ID is required.");
        }
    }
}
