using IslaNova.Core.Application.Dtos.Base;
using IslaNova.Core.Application.Dtos.User;

namespace IslaNova.Core.Application.Dtos.Auth
{
    public class LoginResponseDto : BaseResponseDto
    {
        public required UserDto User { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

    }
}
