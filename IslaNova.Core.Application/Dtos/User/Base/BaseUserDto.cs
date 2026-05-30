namespace IslaNova.Core.Application.Dtos.User.Base
{
    public class BaseUserDto
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string IdentificationNumber { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string PhoneNumber { get; set; }
    }
}


