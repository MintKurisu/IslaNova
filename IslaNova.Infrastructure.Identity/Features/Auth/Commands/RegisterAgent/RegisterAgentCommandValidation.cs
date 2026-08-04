using FluentValidation;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Commands.RegisterAgent
{
    public class RegisterAgentCommandValidation : AbstractValidator<RegisterAgentCommand>
    {
        public RegisterAgentCommandValidation()
        {
            // Identity validations

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress();

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?\d{8,15}$")
                .WithMessage("Invalid phone format");

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty().WithMessage("Identification number is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6);

            // Application validations

            RuleFor(x => x.ProfessionalStatement)
                .NotEmpty().WithMessage("Professional statement is required")
                .MaximumLength(1500);

            RuleFor(x => x.LicenseNumber)
                .MaximumLength(100);

            RuleFor(x => x.CertificationNumber)
                .MaximumLength(100);

            RuleFor(x => x.AgencyName)
                .MaximumLength(200);

            RuleFor(x => x.EmploymentType)
                .IsInEnum()
                .WithMessage("Invalid employment type");
        }
    }
}
