using FluentValidation;

namespace IslaNova.Core.Application.Features.PropertyType.Commands.DeletePropertyType
{
    public class DeletePropertyTypeCommandValidation : AbstractValidator<DeletePropertyTypeCommand>
    {
        public DeletePropertyTypeCommandValidation()
        {
            RuleFor(pt => pt.PropertyTypeId)
           .NotNull().WithMessage("Property type ID is required.")
           .GreaterThan(0).WithMessage("Property type ID must be greater than 0.")
           .WithMessage("Property type ID is required.");
        }
    }
}
