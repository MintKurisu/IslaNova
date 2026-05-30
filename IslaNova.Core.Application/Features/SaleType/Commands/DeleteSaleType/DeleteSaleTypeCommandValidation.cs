using FluentValidation;

namespace IslaNova.Core.Application.Features.SaleType.Commands.DeleteSaleType
{
    public class DeleteSaleTypeCommandValidation : AbstractValidator<DeleteSaleTypeCommand>
    {
        public DeleteSaleTypeCommandValidation()
        {
            RuleFor(st => st.SaleTypeId)
                     .NotNull().WithMessage("Sale type ID is required.")
                     .GreaterThan(0).WithMessage("Sale type ID must be greater than 0.")
                     .WithMessage("Sale type ID is required.");
        }
    }
}
