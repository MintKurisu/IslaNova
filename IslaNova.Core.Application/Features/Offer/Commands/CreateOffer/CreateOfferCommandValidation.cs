using FluentValidation;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Offer.Commands.CreateOffer
{
    public class CreateOfferCommandValidation : AbstractValidator<CreateOfferCommand>
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAuthServiceForWebApi _authService;

        public CreateOfferCommandValidation(
            IOfferRepository offerRepository,
            IPropertyRepository propertyRepository,
            IAuthServiceForWebApi authService)
        {
            _offerRepository = offerRepository;
            _propertyRepository = propertyRepository;
            _authService = authService;

            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(p => p.PropertyId)
                .GreaterThan(0).WithMessage("Property ID must be greater than 0.")
                .MustAsync(ExistProperty).WithMessage("The specified property does not exist.")
                .MustAsync(BeAvailable).WithMessage("Cannot place an offer. This property is already sold or unavailable."); // <-- Nueva Barrera

            RuleFor(p => p.ClientId)
                .NotEmpty().WithMessage("Client ID is required.")
                .MustAsync(ExistInIdentity).WithMessage("The specified Client ID does not exist in the system.")
                .MustAsync(async (command, clientId, cancellationToken) =>
                    !await IsAgentOfProperty(clientId, command.PropertyId, cancellationToken))
                .WithMessage("An Agent cannot make an offer on their own property.");

            RuleFor(p => p.Amount)
                .GreaterThan(0).WithMessage("Offer amount must be greater than 0.");
        }

        private async Task<bool> ExistProperty(int id, CancellationToken cancellationToken)
        {
            return await _propertyRepository
                .GetAllQuery()
                .AnyAsync(p => p.PropertyId == id, cancellationToken);
        }

        private async Task<bool> BeAvailable(int propertyId, CancellationToken cancellationToken)
        {
            // Puedes cambiar '1' por el valor de tu Enum que represente 'Accepted'
            var hasAcceptedOffer = await _offerRepository.GetAllQuery()
                .AnyAsync(o => o.PropertyId == propertyId && o.Status == OfferStatus.Accepted, cancellationToken);
            return !hasAcceptedOffer;
        }

        private async Task<bool> ExistInIdentity(string clientId, CancellationToken cancellationToken)
        {
            var user = await _authService.GetUserById(clientId);
            return user != null;
        }

        private async Task<bool> IsAgentOfProperty(string clientId, int propertyId, CancellationToken cancellationToken)
        {
            var property = await _propertyRepository.GetAllQuery()
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId, cancellationToken);

            if (property == null) return true;
            return property.AgentId == clientId;
        }
    }
}
