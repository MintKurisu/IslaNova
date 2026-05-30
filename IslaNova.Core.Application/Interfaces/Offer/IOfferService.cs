using IslaNova.Core.Application.Dtos.Offer;
using IslaNova.Core.Application.Interfaces.Base;

namespace IslaNova.Core.Application.Interfaces.Offer
{
    public interface IOfferService : IGenericService<OfferDto>
    {
        Task<List<OfferDto>> GetOffersByPropertyIdAsync(int propertyId);
        Task<List<OfferDto>> GetOffersByClientIdAsync(string clientId, int propertyId);
        Task<List<OfferDto>> GetOffersByClientForPropertyAsync(string clientId, int propertyId);
        Task<OfferDto?> CreateOfferAsync(CreateOfferDto dto);
        Task<bool> AcceptOfferAsync(int offerId, int propertyId);
        Task<bool> RejectOfferAsync(int offerId);
        Task<bool> HasPendingOfferForClientAsync(string clientId, int propertyId);
        Task<bool> HasAcceptedOfferForPropertyAsync(int propertyId);
        Task<List<string>> GetClientsWithOffersByPropertyIdAsync(int propertyId);
    }
}
