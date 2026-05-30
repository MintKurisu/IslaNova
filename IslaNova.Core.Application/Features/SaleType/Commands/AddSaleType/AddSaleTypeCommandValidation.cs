using FluentValidation;

namespace IslaNova.Core.Application.Features.SaleType.Commands.AddSaleType
{
    public class AddSaleTypeCommandValidation : AbstractValidator<AddSaleTypeCommand>
    {
        public AddSaleTypeCommandValidation()
        {
            RuleFor(pt => pt.Name)
               .NotEmpty().WithMessage("Sale type name is required.")
               .MaximumLength(100).WithMessage("Sale type name must not exceed 100 characters.");

            RuleFor(pt => pt.Description)
              .NotEmpty().WithMessage("Sale type description is required.")
              .MaximumLength(500).WithMessage("Sale type description must not exceed 500 characters.");
        }
    }
}
