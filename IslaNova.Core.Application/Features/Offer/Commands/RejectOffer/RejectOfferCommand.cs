using FluentValidation;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Offer.Commands.RejectOffer
{
    public class RejectOfferCommand : IRequest<bool>
    {
        [SwaggerParameter(Description = "Offer ID to reject")]
        public int OfferId { get; set; }
    }

    public class RejectOfferCommandHandler : IRequestHandler<RejectOfferCommand, bool>
    {
        private readonly IOfferRepository _offerRepository;

        public RejectOfferCommandHandler(IOfferRepository offerRepository)
        {
            _offerRepository = offerRepository;
        }

        public async Task<bool> Handle(RejectOfferCommand command, CancellationToken cancellationToken)
        {
            var offer = await _offerRepository.GetByIdAsync(command.OfferId);
            if (offer == null || offer.Status != OfferStatus.Pending) return false;

            offer.Status = OfferStatus.Rejected;
            await _offerRepository.UpdateAsync(command.OfferId, offer);

            return true;
        }
    }

}
