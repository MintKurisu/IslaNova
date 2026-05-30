namespace IslaNova.Core.Application.Dtos.Offer
{
    public class CreateOfferDto
    {
        public int PropertyId { get; set; }
        public required string ClientId { get; set; }
        public decimal Amount { get; set; }
    }
}
