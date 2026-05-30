using IslaNova.Core.Application.Dtos.Base;

namespace IslaNova.Core.Application.Dtos.Auth
{
    public class LoginResponseForApiDto : BaseResponseDto
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public string? AccessToken { get; set; }
    }
}
