using AutoMapper;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.Offer;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Interfaces.Offer;
using IslaNova.Core.Application.Interfaces.Property;
using IslaNova.Core.Application.Services.Base;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.UserInteraction;

namespace IslaNova.Core.Application.Services.Offer
{
    public class OfferService : GenericService<Core.Domain.Entities.UserInteraction.Offer, OfferDto>, IOfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IPropertyService _propertyService;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public OfferService(
            IOfferRepository offerRepository,
            IPropertyService propertyService,
            IAuthServiceForWebApi authService,
            IMapper mapper) : base(offerRepository, mapper)
        {
            _offerRepository = offerRepository;
            _propertyService = propertyService;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<List<OfferDto>> GetOffersByPropertyIdAsync(int propertyId)
        {
            try
            {
                var offers = await _offerRepository
                    .GetAllQuery()
                    .Where(o => o.PropertyId == propertyId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var offerDtos = new List<OfferDto>();

                foreach (var offer in offers)
                {
                    var client = await _authService.GetUserById(offer.ClientId);
                    var dto = _mapper.Map<OfferDto>(offer);

                    if (client != null)
                    {
                        dto.ClientName = $"{client.Name} {client.LastName}";
                    }

                    offerDtos.Add(dto);
                }

                return offerDtos;
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<List<OfferDto>> GetOffersByClientIdAsync(string clientId, int propertyId)
        {
            try
            {
                var offers = await _offerRepository
                    .GetAllQuery()
                    .Where(o => o.ClientId == clientId && o.PropertyId == propertyId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<OfferDto>>(offers);
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<List<OfferDto>> GetOffersByClientForPropertyAsync(string clientId, int propertyId)
        {
            try
            {
                var offers = await _offerRepository
                    .GetAllQuery()
                    .Where(o => o.ClientId == clientId && o.PropertyId == propertyId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<OfferDto>>(offers);
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<OfferDto?> CreateOfferAsync(CreateOfferDto dto)
        {
            try
            {
                // Check if there's already an accepted offer for this property
                var hasAcceptedOffer = await HasAcceptedOfferForPropertyAsync(dto.PropertyId);
                if (hasAcceptedOffer) return null;

                // Check if client has a pending offer for this property
                var hasPendingOffer = await HasPendingOfferForClientAsync(dto.ClientId, dto.PropertyId);
                if (hasPendingOffer) return null;

                var offer = new Core.Domain.Entities.UserInteraction.Offer
                {
                    PropertyId = dto.PropertyId,
                    ClientId = dto.ClientId,
                    Amount = dto.Amount,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                var createdOffer = await _offerRepository.AddAsync(offer);
                return _mapper.Map<OfferDto>(createdOffer);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> AcceptOfferAsync(int offerId, int propertyId)
        {
            try
            {
                var offer = await _offerRepository.GetByIdAsync(offerId);
                if (offer == null || offer.Status != OfferStatus.Pending) return false;

                // Accept the offer
                offer.Status = OfferStatus.Accepted;
                await _offerRepository.UpdateAsync(offerId, offer);

                // Reject all other pending offers for this property
                var pendingOffers = await _offerRepository
                    .GetAllQuery()
                    .Where(o => o.PropertyId == propertyId && o.Status == OfferStatus.Pending && o.OfferId != offerId)
                    .ToListAsync();

                foreach (var pendingOffer in pendingOffers)
                {
                    pendingOffer.Status = OfferStatus.Rejected;
                    await _offerRepository.UpdateAsync(pendingOffer.OfferId, pendingOffer);
                }

                // Mark property as sold
                await _propertyService.MarkPropertyAsSoldAsync(propertyId);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RejectOfferAsync(int offerId)
        {
            try
            {
                var offer = await _offerRepository.GetByIdAsync(offerId);
                if (offer == null || offer.Status != OfferStatus.Pending) return false;

                offer.Status = OfferStatus.Rejected;
                await _offerRepository.UpdateAsync(offerId, offer);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> HasPendingOfferForClientAsync(string clientId, int propertyId)
        {
            try
            {
                return await _offerRepository
                    .GetAllQuery()
                    .AnyAsync(o => o.ClientId == clientId && o.PropertyId == propertyId && o.Status == OfferStatus.Pending);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> HasAcceptedOfferForPropertyAsync(int propertyId)
        {
            try
            {
                return await _offerRepository
                    .GetAllQuery()
                    .AnyAsync(o => o.PropertyId == propertyId && o.Status == OfferStatus.Accepted);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<string>> GetClientsWithOffersByPropertyIdAsync(int propertyId)
        {
            try
            {
                return await _offerRepository
                    .GetAllQuery()
                    .Where(o => o.PropertyId == propertyId)
                    .Select(o => o.ClientId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}
