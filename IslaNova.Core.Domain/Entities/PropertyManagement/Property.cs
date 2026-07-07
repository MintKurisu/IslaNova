using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Core.Domain.Entities.UserInteraction;

namespace IslaNova.Core.Domain.Entities.PropertyManagement
{
    public class Property
    {
        public int PropertyId { get; set; }
        public required string Code { get; set; }
        public int PropertyTypeId { get; set; }
        public int SaleTypeId { get; set; }
        public decimal Price { get; set; }
        public double LandSize { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public required string Description { get; set; }
        public required string AgentId { get; set; }
        public PropertyStatus Status { get; set; } = PropertyStatus.Available;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // To filter by most recent to older

        // Location
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }


        // Navigation properties...

        public PropertyType? PropertyType { get; set; }
        public SaleType? SaleType { get; set; }
        public ICollection<PropertyImage>? Images { get; set; }
        public ICollection<PropertyImprovement>? PropertyImprovements { get; set; }
        public ICollection<Offer>? Offers { get; set; }

    }
}
