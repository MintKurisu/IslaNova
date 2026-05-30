namespace IslaNova.Core.Application.Dtos.Auth
{
    public class ResetPasswordRequestDto
    {
        public required string Id { get; set; }
        public required string Password { get; set; }
        public required string Token { get; set; }
    }
}
