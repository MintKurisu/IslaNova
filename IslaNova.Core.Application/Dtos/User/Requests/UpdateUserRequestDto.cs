using System.ComponentModel.DataAnnotations;

namespace IslaNova.Core.Application.Dtos.User.Requests
{
    public class UpdateUserRequestDto
    {
        [Required(ErrorMessage = "Name of user is required")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Last name of user is required\"")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "identification number of the user is required.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "The identification number must contain exactly 11 digits.")]
        public required string IdentificationNumber { get; set; }

        [Required(ErrorMessage = "Email of user is required")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Username of user is required.")]
        public required string UserName { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Password must match")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

    }
}
