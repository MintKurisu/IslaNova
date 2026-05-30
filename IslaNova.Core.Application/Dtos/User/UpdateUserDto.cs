using IslaNova.Core.Application.Dtos.User.Base;

namespace IslaNova.Core.Application.Dtos.User
{
    public class UpdateUserDto : BaseUserDto
    {
        public required string Id { get; set; }
        public required string Password { get; set; }
        public string? ProfileImage { get; set; }

    }
}
