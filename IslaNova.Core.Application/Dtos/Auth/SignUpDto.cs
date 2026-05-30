namespace IslaNova.Core.Application.Dtos.Auth
{
    public class SignUpDto
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public required string IdentificationNumber { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }
        public required string ProfileImage { get; set; }
        public required string Role { get; set; }


    }
}
