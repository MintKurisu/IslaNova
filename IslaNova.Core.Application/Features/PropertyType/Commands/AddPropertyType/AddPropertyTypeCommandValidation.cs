using FluentValidation;

namespace IslaNova.Core.Application.Features.PropertyType.Commands.AddPropertyType
{
    public class AddPropertyTypeCommandValidation : AbstractValidator<AddPropertyTypeCommand>
    {
        public AddPropertyTypeCommandValidation()
        {
            RuleFor(pt => pt.Name)
               .NotEmpty().WithMessage("Property type name is required.")
               .MaximumLength(100).WithMessage("Property type name must not exceed 100 characters.");

            RuleFor(pt => pt.Description)
              .NotEmpty().WithMessage("Property type description is required.")
              .MaximumLength(500).WithMessage("Property type description must not exceed 500 characters.");
        }
    }
}
