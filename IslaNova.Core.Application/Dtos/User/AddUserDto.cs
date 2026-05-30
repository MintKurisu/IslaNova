using IslaNova.Core.Application.Dtos.User.Base;

namespace IslaNova.Core.Application.Dtos.User
{
    public class AddUserDto : BaseUserDto
    {
        public required string Password { get; set; }
        public required string Role { get; set; }
        public string? ProfileImage { get; set; }

    }
}


