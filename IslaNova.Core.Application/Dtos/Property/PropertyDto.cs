namespace IslaNova.Core.Application.Dtos.Property
{
    public class PropertyDto
    {
        public int PropertyId { get; set; }
        public required string Code { get; set; }
        public int PropertyTypeId { get; set; }
        public string? PropertyTypeName { get; set; }
        public int SaleTypeId { get; set; }
        public string? SaleTypeName { get; set; }
        public decimal Price { get; set; }
        public double LandSize { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public required string Description { get; set; }
        public required string AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? AgentEmail { get; set; }
        public string? AgentPhone { get; set; }
        public string? AgentProfileImage { get; set; }
        public string Status { get; set; } = "Available";
        public DateTime CreatedAt { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<string>? ImprovementNames { get; set; }
    }
}
