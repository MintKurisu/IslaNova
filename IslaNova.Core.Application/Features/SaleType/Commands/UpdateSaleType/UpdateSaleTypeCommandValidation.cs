using FluentValidation;

namespace IslaNova.Core.Application.Features.SaleType.Commands.UpdateSaleType
{
    public class UpdateSaleTypeCommandValidation : AbstractValidator<UpdateSaleTypeCommand>
    {
        public UpdateSaleTypeCommandValidation()
        {
            RuleFor(st => st.SaleTypeId)
                .NotNull().WithMessage("Sale type ID is required.")
                .GreaterThan(0).WithMessage("Sale type ID must be greater than 0.")
                .WithMessage("Sale type ID is required.");

            RuleFor(pt => pt.Name)
                .NotEmpty().WithMessage("Sale type name is required.")
                .MaximumLength(100).WithMessage("Sale type name must not exceed 100 characters.");

            RuleFor(pt => pt.Description)
               .NotEmpty().WithMessage("Sale type description is required.")
               .MaximumLength(500).WithMessage("Sale type description must not exceed 500 characters.");
        }
    }
}
