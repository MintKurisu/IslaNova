namespace IslaNova.Core.Application.Dtos.Offer
{
    public class CreateOfferDto
    {
        public int PropertyId { get; set; }
        public required string ContactName { get; set; }
        public required string ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public decimal Amount { get; set; }
    }
}
