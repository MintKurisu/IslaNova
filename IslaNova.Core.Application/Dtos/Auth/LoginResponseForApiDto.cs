using IslaNova.Core.Application.Dtos.Base;
using IslaNova.Core.Domain.Common.Enums;

namespace IslaNova.Core.Application.Dtos.Auth
{
    public class LoginResponseForApiDto : BaseResponseDto
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public Roles? Role { get; set; }
        public string? ProfileImage { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
