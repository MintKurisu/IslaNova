using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Core.Domain.Entities.UserInteraction
{
    public class Favorite
    {
        public int FavoriteId { get; set; }
        public required string ClientId { get; set; }
        public int PropertyId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Not necessary on the requirement, but useful

        // Navigation Properties...

        public Property? Property { get; set; }
    }
}
