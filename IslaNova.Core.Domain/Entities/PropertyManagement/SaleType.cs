namespace IslaNova.Core.Domain.Entities.PropertyManagement
{
    public class SaleType
    {
        public int SaleTypeId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }

        // Navigation Properties...

        public ICollection<Property>? Properties { get; set; }
    }
}
