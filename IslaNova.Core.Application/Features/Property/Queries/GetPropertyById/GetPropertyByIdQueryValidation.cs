using FluentValidation;

namespace IslaNova.Core.Application.Features.Property.Queries.GetPropertyById
{
    public class GetPropertyByIdQueryValidation : AbstractValidator<GetPropertyByIdQuery>
    {
        public GetPropertyByIdQueryValidation()
        {
            RuleFor(p => p.PropertyId)
              .NotNull().WithMessage("Property ID is required.")
              .GreaterThan(0).WithMessage("Property ID must be greater than 0.")
              .WithMessage("Property ID is required.");
        }
    }
}
