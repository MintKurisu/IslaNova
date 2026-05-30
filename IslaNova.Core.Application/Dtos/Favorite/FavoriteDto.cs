namespace IslaNova.Core.Application.Dtos.Favorite
{
    public class FavoriteDto
    {
        public int FavoriteId { get; set; }
        public required string ClientId { get; set; }
        public int PropertyId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
