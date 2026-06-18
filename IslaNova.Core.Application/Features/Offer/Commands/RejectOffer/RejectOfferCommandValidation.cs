using FluentValidation;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Offer.Commands.RejectOffer
{
    public class RejectOfferCommandValidation : AbstractValidator<RejectOfferCommand>
    {
        private readonly IOfferRepository _offerRepository;

        public RejectOfferCommandValidation(IOfferRepository offerRepository)
        {
            _offerRepository = offerRepository;

            RuleFor(p => p.OfferId)
                .GreaterThan(0).WithMessage("Offer ID must be greater than 0.")
                .MustAsync(ExistOffer).WithMessage("The specified offer does not exist.");
        }

        private async Task<bool> ExistOffer(int id, CancellationToken cancellationToken)
        {
            return await _offerRepository
                .GetAllQuery()
                .AnyAsync(o => o.OfferId == id, cancellationToken);
        }
    }
}
