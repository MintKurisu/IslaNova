using IslaNova.Core.Application.Dtos.Base;

namespace IslaNova.Core.Application.Dtos.User
{
    public class RegisterAgentResponseDto : BaseResponseDto
    {
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
    }
}
