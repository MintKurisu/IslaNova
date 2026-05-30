namespace IslaNova.Core.Domain.Entities.PropertyManagement
{
    public class PropertyImage
    {
        public int PropertyImageId { get; set; }
        public int PropertyId { get; set; }
        public required string ImageUrl { get; set; }

        // Navigation Properties...

        public Property? Property { get; set; }
    }
}
