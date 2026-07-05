using FluentValidation;
using IslaNova.Core.Domain.Enums;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using Microsoft.EntityFrameworkCore;


namespace IslaNova.Core.Application.Features.AgentApplication.Commands.RejectAgentApplication
{
    public class RejectAgentApplicationCommandValidation : AbstractValidator<RejectAgentApplicationCommand>
    {
        private readonly IAgentApplicationRepository _repository;

        public RejectAgentApplicationCommandValidation(IAgentApplicationRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.ApplicationId)
                .GreaterThan(0).WithMessage("Application ID must be greater than 0.")
                .MustAsync(ExistAndBePending).WithMessage("Application not found or is not in Pending status.");

            RuleFor(x => x.AdminComments)
                .MaximumLength(1000).WithMessage("Admin comments cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.AdminComments));
        }

        private async Task<bool> ExistAndBePending(int id, CancellationToken cancellationToken)
        {
            return await _repository.GetAllQuery()
                .AnyAsync(a => a.ApplicationId == id && a.Status == ApplicationStatus.Pending, cancellationToken);
        }
    }
}
