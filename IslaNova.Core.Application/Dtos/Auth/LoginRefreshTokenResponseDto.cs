using IslaNova.Core.Application.Dtos.Base;

namespace IslaNova.Core.Application.Dtos.Auth
{
    public class LoginRefreshTokenResponseDto : BaseResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
