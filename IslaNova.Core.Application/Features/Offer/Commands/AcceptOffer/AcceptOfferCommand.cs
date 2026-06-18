using FluentValidation;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Offer.Commands.AcceptOffer
{
    public class AcceptOfferCommand : IRequest<bool>
    {
        [SwaggerParameter(Description = "Offer ID to accept")]
        public int OfferId { get; set; }
        [SwaggerParameter(Description = "Property ID associated with the offer")]
        public int PropertyId { get; set; }
    }

    public class AcceptOfferCommandHandler : IRequestHandler<AcceptOfferCommand, bool>
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IPropertyRepository _propertyRepository;

        public AcceptOfferCommandHandler(
            IOfferRepository offerRepository,
            IPropertyRepository propertyRepository)
        {
            _offerRepository = offerRepository;
            _propertyRepository = propertyRepository;
        }

        public async Task<bool> Handle(AcceptOfferCommand command, CancellationToken cancellationToken)
        {
            var offer = await _offerRepository.GetByIdAsync(command.OfferId);
            if (offer == null || offer.Status != OfferStatus.Pending) return false;

            offer.Status = OfferStatus.Accepted;
            await _offerRepository.UpdateAsync(command.OfferId, offer);

            var pendingOffers = await _offerRepository
                .GetAllQuery()
                .Where(o => o.PropertyId == command.PropertyId
                    && o.Status == OfferStatus.Pending
                    && o.OfferId != command.OfferId)
                .ToListAsync(cancellationToken);

            foreach (var pendingOffer in pendingOffers)
            {
                pendingOffer.Status = OfferStatus.Rejected;
                await _offerRepository.UpdateAsync(pendingOffer.OfferId, pendingOffer);
            }

            var property = await _propertyRepository.GetByIdAsync(command.PropertyId);
            if (property != null)
            {
                property.Status = PropertyStatus.Sold;
                await _propertyRepository.UpdateAsync(command.PropertyId, property);
            }

            return true;
        }
    }

}
