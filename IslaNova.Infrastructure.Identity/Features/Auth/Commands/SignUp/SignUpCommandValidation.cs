using FluentValidation;
using IslaNova.Core.Domain.Common.Enums;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Commands.SignUp
{
    public class SignUpCommandValidation : AbstractValidator<SignUpCommand>
    {
        public SignUpCommandValidation()
        {
            RuleFor(x => x.Name)
                 .NotEmpty().WithMessage("Name is required")
                 .MaximumLength(50).WithMessage("Name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
                .MaximumLength(30).WithMessage("Username cannot exceed 30 characters");

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty().WithMessage("Identification number is required")
                .Matches(@"^\d{11}$").WithMessage("Identification number must contain exactly 11 digits");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?\d{8,15}$")
                .WithMessage("Phone number must contain only numbers and be 8 to 15 digits long");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(role =>
                    !string.IsNullOrWhiteSpace(role) &&
                    role.Equals(Roles.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
                .WithMessage("Role is invalid or not allowed.");


        }
    }
}
