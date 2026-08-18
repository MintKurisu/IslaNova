using IslaNova.Core.Application.Dtos.User.Base;
using Microsoft.AspNetCore.Http;

namespace IslaNova.Core.Application.Dtos.User
{
    public class UpdateUserDto : BaseUserDto
    {
        public required string Id { get; set; }
        public required string Password { get; set; }
        public IFormFile? ProfileImageFile { get; set; }

    }
}
