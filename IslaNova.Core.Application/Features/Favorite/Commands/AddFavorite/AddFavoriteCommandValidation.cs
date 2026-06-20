using FluentValidation;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Favorite.Commands.AddFavorite
{
    public class AddFavoriteCommandValidation : AbstractValidator<AddFavoriteCommand>
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAuthServiceForWebApi _authService;

        public AddFavoriteCommandValidation(
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
                .MustAsync(ExistInIdentity).WithMessage("The specified Client ID does not exist in the system.")
                .MustAsync(BeCustomer).WithMessage("Only customers can add properties to favorites.");

            RuleFor(p => p.PropertyId)
                .GreaterThan(0).WithMessage("Property ID must be greater than 0.")
                .MustAsync(ExistProperty).WithMessage("The specified property does not exist.")
                .MustAsync(BeAvailable).WithMessage("Cannot favorite a sold property.")
                .MustAsync((command, propertyId, cancellationToken) =>
                    NotAlreadyFavorite(command.ClientId ?? "", propertyId, cancellationToken))
                .WithMessage("This property is already in the client's favorites.");
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

        private async Task<bool> ExistProperty(int propertyId, CancellationToken cancellationToken)
        {
            return await _propertyRepository
                .GetAllQuery()
                .AnyAsync(p => p.PropertyId == propertyId, cancellationToken);
        }

        private async Task<bool> BeAvailable(int propertyId, CancellationToken cancellationToken)
        {
            return await _propertyRepository
                .GetAllQuery()
                .AnyAsync(p => p.PropertyId == propertyId && p.Status == PropertyStatus.Available, cancellationToken);
        }

        private async Task<bool> NotAlreadyFavorite(string clientId, int propertyId, CancellationToken cancellationToken)
        {
            return !await _favoriteRepository
                .GetAllQuery()
                .AnyAsync(f => f.ClientId == clientId && f.PropertyId == propertyId, cancellationToken);
        }
    }
}
