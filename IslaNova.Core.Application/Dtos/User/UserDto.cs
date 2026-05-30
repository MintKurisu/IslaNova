using IslaNova.Core.Application.Dtos.User.Base;

namespace IslaNova.Core.Application.Dtos.User
{
    public class UserDto : BaseUserDto
    {
        public required string Id { get; set; }
        public required string Role { get; set; }
        public bool IsActive { get; set; }
        public string? ProfileImage { get; set; }

    }
}


