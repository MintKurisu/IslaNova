using FluentValidation;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Favorite.Commands.RemoveFavorite
{
    public class RemoveFavoriteCommandValidation : AbstractValidator<RemoveFavoriteCommand>
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAuthServiceForWebApi _authService;

        public RemoveFavoriteCommandValidation(
            IFavoriteRepository favoriteRepository,
            IPropertyRepository propertyRepository,
            IAuthServiceForWebApi authService)
        {
            _favoriteRepository = favoriteRepository;
            _propertyRepository = propertyRepository;
            _authService = authService;

            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(p => p.ClientId)
                .NotEmpty().WithMessage("Client ID is required.")
                .MustAsync(ExistInIdentity).WithMessage("The specified Client ID does not exist in the system.");

            RuleFor(p => p.PropertyId)
                .GreaterThan(0).WithMessage("Property ID must be greater than 0.")
                .MustAsync(ExistProperty).WithMessage("The specified property does not exist.")
                .MustAsync((command, propertyId, cancellationToken) =>
                    IsFavorite(command.ClientId ?? "", propertyId, cancellationToken))
                .WithMessage("This property is not in the client's favorites.");
        }

        private async Task<bool> ExistInIdentity(string? clientId, CancellationToken cancellationToken)
        {
            var user = await _authService.GetUserById(clientId ?? "");
            return user != null;
        }

        private async Task<bool> ExistProperty(int propertyId, CancellationToken cancellationToken)
        {
            return await _propertyRepository
                .GetAllQuery()
                .AnyAsync(p => p.PropertyId == propertyId, cancellationToken);
        }

        private async Task<bool> IsFavorite(string clientId, int propertyId, CancellationToken cancellationToken)
        {
            return await _favoriteRepository
                .GetAllQuery()
                .AnyAsync(f => f.ClientId == clientId && f.PropertyId == propertyId, cancellationToken);
        }
    }
}
