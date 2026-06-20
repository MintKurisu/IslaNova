using FluentValidation;
using IslaNova.Core.Application.Interfaces.Auth;

namespace IslaNova.Core.Application.Features.Favorite.Queries.GetClientFavoriteProperties
{
    public class GetClientFavoritePropertiesQueryValidation : AbstractValidator<GetClientFavoritePropertiesQuery>
    {
        private readonly IAuthServiceForWebApi _authService;

        public GetClientFavoritePropertiesQueryValidation(IAuthServiceForWebApi authService)
        {
            _authService = authService;

            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(p => p.ClientId)
                .NotEmpty().WithMessage("Client ID is required.")
                .MustAsync(ExistInIdentity).WithMessage("The specified Client ID does not exist in the system.")
                .MustAsync(BeCustomer).WithMessage("Only customers can have favorite properties.");
        }

        private async Task<bool> ExistInIdentity(string? clientId, CancellationToken cancellationToken)
        {
            var user = await _authService.GetUserById(clientId ?? "");
            return user != null;
        }

        private async Task<bool> BeCustomer(string? clientId, CancellationToken cancellationToken)
        {
            var user = await _authService.GetUserById(clientId ?? "");
            return user != null && user.Role == "Customer";
        }
    }
}
