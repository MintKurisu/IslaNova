using AutoMapper;
using FluentValidation;
using IslaNova.Core.Application.Dtos.Offer;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Offer.Commands.CreateOffer
{
    public class CreateOfferCommand : IRequest<OfferDto?>
    {
        [SwaggerParameter(Description = "Property ID to make an offer on")]
        public int PropertyId { get; set; }
        [SwaggerParameter(Description = "Client ID making the offer")]
        public string? ClientId { get; set; }
        [SwaggerParameter(Description = "Offer amount in DOP")]
        public decimal Amount { get; set; }
    }

    public class CreateOfferCommandHandler : IRequestHandler<CreateOfferCommand, OfferDto?>
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IMapper _mapper;

        public CreateOfferCommandHandler(
            IOfferRepository offerRepository,
            IMapper mapper)
        {
            _offerRepository = offerRepository;
            _mapper = mapper;
        }

        public async Task<OfferDto?> Handle(CreateOfferCommand command, CancellationToken cancellationToken)
        {
            var hasAcceptedOffer = await _offerRepository
                .GetAllQuery()
                .AnyAsync(o => o.PropertyId == command.PropertyId && o.Status == OfferStatus.Accepted, cancellationToken);

            if (hasAcceptedOffer) return null;

            var hasPendingOffer = await _offerRepository
                .GetAllQuery()
                .AnyAsync(o => o.ClientId == command.ClientId && o.PropertyId == command.PropertyId && o.Status == OfferStatus.Pending, cancellationToken);

            if (hasPendingOffer) return null;

            var offer = new Domain.Entities.UserInteraction.Offer
            {
                PropertyId = command.PropertyId,
                ClientId = command.ClientId ?? "",
                Amount = command.Amount,
                Status = OfferStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var createdOffer = await _offerRepository.AddAsync(offer);
            return _mapper.Map<OfferDto>(createdOffer);
        }
    }
}
