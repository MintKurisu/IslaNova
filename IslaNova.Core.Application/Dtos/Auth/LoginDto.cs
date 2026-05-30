namespace IslaNova.Core.Application.Dtos.Auth
{
    public class LoginDto
    {
        public required string Identifier { get; set; }
        public required string Password { get; set; }
    }
}
