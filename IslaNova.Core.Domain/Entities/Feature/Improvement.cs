namespace IslaNova.Core.Domain.Entities.Feature
{
    public class Improvement
    {
        public int ImprovementId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }

        // Navigation Properties...

        public ICollection<PropertyImprovement>? PropertyImprovements { get; set; }
    }
}
