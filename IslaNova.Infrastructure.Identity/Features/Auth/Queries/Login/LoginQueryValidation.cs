using FluentValidation;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Queries.Login
{
    public class LoginQueryValidation : AbstractValidator<LoginQuery>
    {
        public LoginQueryValidation()
        {
            RuleFor(l => l.Identifier)
                .NotNull().WithMessage("Identifier is required")
                .NotEmpty().WithMessage("Identifier is required");

            RuleFor(l => l.Password)
              .NotNull().WithMessage("Password is required")
              .NotEmpty().WithMessage("Password is required");
        }
    }
}
