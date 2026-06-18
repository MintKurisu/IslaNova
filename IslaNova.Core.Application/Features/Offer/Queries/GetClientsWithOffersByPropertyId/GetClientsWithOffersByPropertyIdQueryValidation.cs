using FluentValidation;

namespace IslaNova.Core.Application.Features.Offer.Queries.GetClientsWithOffersByPropertyId
{
    public class GetClientsWithOffersByPropertyIdQueryValidation : AbstractValidator<GetClientsWithOffersByPropertyIdQuery>
    {
        public GetClientsWithOffersByPropertyIdQueryValidation()
        {
            RuleFor(p => p.PropertyId)
                .GreaterThan(0).WithMessage("Property ID must be greater than 0.");
        }
    }
}
