namespace IslaNova.Core.Application.Dtos.Property
{
    public class CreatePropertyDto
    {
        public int PropertyTypeId { get; set; }
        public int SaleTypeId { get; set; }
        public decimal Price { get; set; }
        public double LandSize { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public required string Description { get; set; }
        public required string AgentId { get; set; }
        public List<int>? ImprovementIds { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
}
