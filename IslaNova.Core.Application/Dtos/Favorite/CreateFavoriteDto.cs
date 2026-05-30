namespace IslaNova.Core.Application.Dtos.Favorite
{
    public class CreateFavoriteDto
    {
        public required string ClientId { get; set; }
        public int PropertyId { get; set; }
    }
}
