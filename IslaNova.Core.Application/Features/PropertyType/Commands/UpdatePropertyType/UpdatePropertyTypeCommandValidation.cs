using FluentValidation;

namespace IslaNova.Core.Application.Features.PropertyType.Commands.UpdatePropertyType
{
    public class UpdatePropertyTypeCommandValidation : AbstractValidator<UpdatePropertyTypeCommand>
    {
        public UpdatePropertyTypeCommandValidation()
        {
            RuleFor(pt => pt.PropertyTypeId)
            .NotNull().WithMessage("Property type ID is required.")
            .GreaterThan(0).WithMessage("Property type ID must be greater than 0.")
            .WithMessage("Property type ID is required.");

            RuleFor(pt => pt.Name)
              .NotEmpty().WithMessage("Property type name is required.")
              .MaximumLength(100).WithMessage("Property type name must not exceed 100 characters.");

            RuleFor(pt => pt.Description)
              .NotEmpty().WithMessage("Property type description is required.")
              .MaximumLength(500).WithMessage("Property type description must not exceed 500 characters.");
        }
    }
}
