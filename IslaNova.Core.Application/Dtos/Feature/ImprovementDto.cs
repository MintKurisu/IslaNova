namespace IslaNova.Core.Application.Dtos.Feature
{
    // Used to display improvement/amenity information (pool, gym, etc.) in listings and details
    public class ImprovementDto
    {
        public int ImprovementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
