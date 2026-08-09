using FluentValidation;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Queries.Refresh
{
    public class RefreshQueryValidation : AbstractValidator<RefreshQuery>
    {
        public RefreshQueryValidation()
        {
            RuleFor(l => l.refreshTokenRequest)
                .NotNull().WithMessage("refreshToken is required")
                .NotEmpty().WithMessage("refreshToken is required");

        }
    }
}
