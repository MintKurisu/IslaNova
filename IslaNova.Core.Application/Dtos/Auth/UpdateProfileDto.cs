using Microsoft.AspNetCore.Http;

namespace IslaNova.Core.Application.Dtos.Auth
{
    public class UpdateProfileDto
    {
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? ProfileImageFile { get; set; }
        public string? Password { get; set; }
    }
}
