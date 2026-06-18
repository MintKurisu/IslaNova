using FluentValidation;

namespace IslaNova.Core.Application.Features.Property.Queries.FilterProperties
{
    public class FilterPropertiesQueryValidation : AbstractValidator<FilterPropertiesQuery>
    {
        public FilterPropertiesQueryValidation()
        {
            RuleFor(p => p.PropertyTypeId)
                .GreaterThan(0).WithMessage("Property type ID must be greater than 0.")
                .When(p => p.PropertyTypeId.HasValue);

            RuleFor(p => p.MinPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum price cannot be negative.")
                .When(p => p.MinPrice.HasValue);

            RuleFor(p => p.MaxPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Maximum price cannot be negative.")
                .When(p => p.MaxPrice.HasValue);

            RuleFor(p => p.MaxPrice)
                .GreaterThanOrEqualTo(p => p.MinPrice!.Value)
                .WithMessage("Maximum price must be greater than or equal to minimum price.")
                .When(p => p.MinPrice.HasValue && p.MaxPrice.HasValue);

            RuleFor(p => p.Bedrooms)
                .GreaterThanOrEqualTo(0).WithMessage("Bedrooms count cannot be negative.")
                .When(p => p.Bedrooms.HasValue);

            RuleFor(p => p.Bathrooms)
                .GreaterThanOrEqualTo(0).WithMessage("Bathrooms count cannot be negative.")
                .When(p => p.Bathrooms.HasValue);
        }
    }
}