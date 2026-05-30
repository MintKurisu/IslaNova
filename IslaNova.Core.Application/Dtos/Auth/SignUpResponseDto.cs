using IslaNova.Core.Application.Dtos.Base;

namespace IslaNova.Core.Application.Dtos.Auth
{
    public class SignUpResponseDto : BaseResponseDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string IdentificationNumber { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string PhoneNumber { get; set; }
        public string? ProfileImage { get; set; }
        public bool IsVerified { get; set; }
        public List<string>? Roles { get; set; }

    }
}
