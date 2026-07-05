using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Enums;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.AgentApplication.Commands.ApproveAgentApplication
{
    public class ApproveAgentApplicationCommand : IRequest<bool>
    {
        [SwaggerParameter(Description = "Application ID to approve")]
        public int ApplicationId { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        public string? ReviewedBy { get; set; }
    }

    public class ApproveAgentApplicationCommandHandler : IRequestHandler<ApproveAgentApplicationCommand, bool>
    {
        private readonly IAgentApplicationRepository _repository;
        private readonly IAuthServiceForWebApi _authService;

        public ApproveAgentApplicationCommandHandler(
            IAgentApplicationRepository repository,
            IAuthServiceForWebApi authService)
        {
            _repository = repository;
            _authService = authService;
        }

        public async Task<bool> Handle(ApproveAgentApplicationCommand command, CancellationToken cancellationToken)
        {
            var application = await _repository.GetByIdAsync(command.ApplicationId);
            if (application == null || application.Status != ApplicationStatus.Pending)
                return false;

            application.Status = ApplicationStatus.Approved;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedBy = command.ReviewedBy;

            await _repository.UpdateAsync(application.ApplicationId, application);

            // Activate the agent account
            await _authService.ToggleUserStatus(application.UserId, true);

            return true;
        }
    }
}
