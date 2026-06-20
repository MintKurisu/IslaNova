using FluentValidation;

namespace IslaNova.Core.Application.Features.Offer.Queries.GetOffersByPropertyId
{
    public class GetOffersByPropertyIdQueryValidation : AbstractValidator<GetOffersByPropertyIdQuery>
    {
        public GetOffersByPropertyIdQueryValidation()
        {
            RuleFor(p => p.PropertyId)
                .GreaterThan(0).WithMessage("Property ID must be greater than 0.");
        }
    }
}
