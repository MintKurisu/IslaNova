using FluentValidation;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Property.Commands.DeletePropertiesByAgent
{
    public class DeletePropertiesByAgentCommandValidation : AbstractValidator<DeletePropertiesByAgentCommand>
    {
        private readonly IPropertyRepository _propertyRepository;

        public DeletePropertiesByAgentCommandValidation(IPropertyRepository propertyRepository)
        {
            _propertyRepository = propertyRepository;

            RuleFor(p => p.AgentId)
                .NotEmpty().WithMessage("Agent ID is required.")
                .MustAsync(AgentHasProperties).WithMessage("The specified Agent has no properties to delete.");
        }

        private async Task<bool> AgentHasProperties(string? agentId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(agentId))
                return false;

            return await _propertyRepository.GetAllQuery().AnyAsync(p => p.AgentId == agentId, cancellationToken);
        }
    }
}
