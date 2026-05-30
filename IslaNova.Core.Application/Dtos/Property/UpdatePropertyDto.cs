namespace IslaNova.Core.Application.Dtos.Property
{
    public class UpdatePropertyDto
    {
        public int PropertyId { get; set; }
        public int PropertyTypeId { get; set; }
        public int SaleTypeId { get; set; }
        public decimal Price { get; set; }
        public double LandSize { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public required string Description { get; set; }
        public List<int>? ImprovementIds { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
}
