namespace IslaNova.Core.Application.Dtos.Offer
{
    public class UpdateOfferStatusDto
    {
        public int OfferId { get; set; }
        public required string Status { get; set; } // Accepted, Rejected
    }
}
