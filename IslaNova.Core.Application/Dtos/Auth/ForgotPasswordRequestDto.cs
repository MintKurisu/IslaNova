namespace IslaNova.Core.Application.Dtos.Auth
{
    public class ForgotPasswordRequestDto
    {
        public required string UserName { get; set; }
        public string? Origin { get; set; }

    }
}
