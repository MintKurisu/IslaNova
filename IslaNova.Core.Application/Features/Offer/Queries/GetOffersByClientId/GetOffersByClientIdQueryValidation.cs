using FluentValidation;

namespace IslaNova.Core.Application.Features.Offer.Queries.GetOffersByClientId
{
    public class GetOffersByClientIdQueryValidation : AbstractValidator<GetOffersByClientIdQuery>
    {
        public GetOffersByClientIdQueryValidation()
        {
            RuleFor(p => p.ClientId)
                .NotEmpty().WithMessage("Client ID is required.");

            RuleFor(p => p.PropertyId)
                .GreaterThan(0).WithMessage("Property ID must be greater than 0.");
        }
    }
}
