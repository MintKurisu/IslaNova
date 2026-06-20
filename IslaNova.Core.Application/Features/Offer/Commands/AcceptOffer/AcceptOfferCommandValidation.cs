using FluentValidation;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Offer.Commands.AcceptOffer
{
    public class AcceptOfferCommandValidation : AbstractValidator<AcceptOfferCommand>
    {
        private readonly IOfferRepository _offerRepository;

        public AcceptOfferCommandValidation(IOfferRepository offerRepository)
        {
            _offerRepository = offerRepository;

            RuleFor(p => p.OfferId)
                .GreaterThan(0).WithMessage("Offer ID must be greater than 0.")
                .MustAsync(ExistOffer).WithMessage("The specified offer does not exist.");

            RuleFor(p => p.PropertyId)
                .GreaterThan(0).WithMessage("Property ID must be greater than 0.");
        }

        private async Task<bool> ExistOffer(int id, CancellationToken cancellationToken)
        {
            return await _offerRepository
                .GetAllQuery()
                .AnyAsync(o => o.OfferId == id, cancellationToken);
        }
    }
}
