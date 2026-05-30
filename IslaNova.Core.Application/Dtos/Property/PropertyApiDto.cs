namespace IslaNova.Core.Application.Dtos.Property
{
    public class PropertyApiDto
    {
        public int PropertyId { get; set; }
        public required string Code { get; set; }
        public required string PropertyTypeName { get; set; }
        public required string SaleTypeName { get; set; }
        public decimal Price { get; set; }
        public double LandSize { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public required string Description { get; set; }
        public List<string>? ImprovementNames { get; set; }
        public string? AgentName { get; set; }
        public required string AgentId { get; set; }
        public required string Status { get; set; }
    }
}
