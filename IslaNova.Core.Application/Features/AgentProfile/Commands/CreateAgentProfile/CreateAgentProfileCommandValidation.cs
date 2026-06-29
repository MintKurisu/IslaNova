using FluentValidation;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.AccountManagement;

namespace IslaNova.Core.Application.Features.AgentProfile.Commands.CreateAgentProfile
{
    public class CreateAgentProfileCommandValidation : AbstractValidator<CreateAgentProfileCommand>
    {
        private readonly IAgentProfileRepository _agentProfileRepository;
        private readonly IAuthServiceForWebApi _authService;

        public CreateAgentProfileCommandValidation(
            IAgentProfileRepository agentProfileRepository,
            IAuthServiceForWebApi authService)
        {
            _agentProfileRepository = agentProfileRepository;
            _authService = authService;

            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(p => p.AgentId)
                .NotEmpty().WithMessage("Agent ID is required.")
                .MustAsync(ExistInIdentity).WithMessage("The specified Agent ID does not exist.")
                .MustAsync(BeAgent).WithMessage("The specified user is not an Agent.")
                .MustAsync(NotHaveProfile).WithMessage("This agent already has a profile.");

            RuleFor(p => p.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage("Years of experience cannot be negative.")
                .LessThanOrEqualTo(60).WithMessage("Years of experience seems too high.")
                .When(p => p.YearsOfExperience.HasValue);

            RuleFor(p => p.Bio)
                .MaximumLength(1000).WithMessage("Bio cannot exceed 1000 characters.")
                .When(p => !string.IsNullOrEmpty(p.Bio));

            RuleFor(p => p.WhatsappNumber)
                .MaximumLength(20).WithMessage("WhatsApp number cannot exceed 20 characters.")
                .Matches(@"^\+?\d{8,15}$").WithMessage("WhatsApp number must be a valid phone number.")
                .When(p => !string.IsNullOrEmpty(p.WhatsappNumber));

            RuleFor(p => p.FacebookUrl)
                .MaximumLength(300).WithMessage("Facebook URL cannot exceed 300 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Facebook URL must be a valid URL.")
                .When(p => !string.IsNullOrEmpty(p.FacebookUrl));

            RuleFor(p => p.InstagramUrl)
                .MaximumLength(300).WithMessage("Instagram URL cannot exceed 300 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Instagram URL must be a valid URL.")
                .When(p => !string.IsNullOrEmpty(p.InstagramUrl));

            RuleFor(p => p.SpecialtyZones)
                .MaximumLength(300).WithMessage("Specialty zones cannot exceed 300 characters.")
                .When(p => !string.IsNullOrEmpty(p.SpecialtyZones));
        }

        private async Task<bool> ExistInIdentity(string? agentId, CancellationToken cancellationToken)
        {
            var user = await _authService.GetUserById(agentId ?? "");
            return user != null;
        }

        private async Task<bool> BeAgent(string? agentId, CancellationToken cancellationToken)
        {
            var user = await _authService.GetUserById(agentId ?? "");
            return user != null && user.Role == "Agent";
        }

        private async Task<bool> NotHaveProfile(string? agentId, CancellationToken cancellationToken)
        {
            var existing = await _agentProfileRepository.GetByAgentIdAsync(agentId ?? "");
            return existing == null;
        }
    }
}
