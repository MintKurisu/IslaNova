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

        public CreateOfferCommandValidation(
            IOfferRepository offerRepository,
            IPropertyRepository propertyRepository)
        {
            _offerRepository = offerRepository;
            _propertyRepository = propertyRepository;

            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(p => p.PropertyId)
                .GreaterThan(0).WithMessage("Property ID must be greater than 0.")
                .MustAsync(ExistProperty).WithMessage("The specified property does not exist.")
                .MustAsync(BeAvailable).WithMessage("Cannot place an offer. This property is already sold or unavailable.");

            RuleFor(p => p.ContactName)
                .NotEmpty().WithMessage("Contact name is required.")
                .MaximumLength(150).WithMessage("Contact name cannot exceed 150 characters.");

            RuleFor(p => p.ContactPhone)
                .NotEmpty().WithMessage("Contact phone is required.")
                .MaximumLength(20).WithMessage("Contact phone cannot exceed 20 characters.")
                .Matches(@"^\+?\d{8,15}$").WithMessage("Contact phone must be a valid phone number.");

            RuleFor(p => p.ContactEmail)
                .EmailAddress().WithMessage("Contact email must be a valid email address.")
                .MaximumLength(256).WithMessage("Contact email cannot exceed 256 characters.")
                .When(p => !string.IsNullOrEmpty(p.ContactEmail));

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
            var hasAcceptedOffer = await _offerRepository
                .GetAllQuery()
                .AnyAsync(o => o.PropertyId == propertyId && o.Status == OfferStatus.Accepted, cancellationToken);
            return !hasAcceptedOffer;
        }
    }
}
