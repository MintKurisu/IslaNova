namespace IslaNova.Core.Application.Dtos.Offer
{
    public class OfferDto
    {
        public int OfferId { get; set; }
        public int PropertyId { get; set; }
        public required string ClientId { get; set; }
        public string? ClientName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
