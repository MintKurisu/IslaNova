using IslaNova.Core.Application.Dtos.Base;

namespace IslaNova.Core.Application.Dtos.User
{
    public class UpdateUserResponseDto : BaseResponseDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string IdentificationNumber { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Role { get; set; }
        public bool IsActive { get; set; }

    }
}
