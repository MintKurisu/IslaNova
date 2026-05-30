using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Core.Domain.Entities.Feature
{
    public class PropertyImprovement
    {
        public int PropertyImprovementId { get; set; }
        public int PropertyId { get; set; }
        public int ImprovementId { get; set; }

        // Navigation Properties...

        public Property? Property { get; set; }
        public Improvement? Improvement { get; set; }
    }
}
