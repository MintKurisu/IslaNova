using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Core.Domain.Entities.UserInteraction
{
    public class Offer
    {
        public int OfferId { get; set; }
        public int PropertyId { get; set; }

        // Anonymous contact info
        public required string ContactName { get; set; }
        public required string ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        public decimal Amount { get; set; }
        public OfferStatus Status { get; set; } = OfferStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties...
        public Property? Property { get; set; }
    }
}
