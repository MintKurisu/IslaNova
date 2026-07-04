using IslaNova.Core.Domain.Enums;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.AgentApplication.Commands.RejectAgentApplication
{
    public class RejectAgentApplicationCommand : IRequest<bool>
    {
        [SwaggerParameter(Description = "Application ID to reject")]
        public int ApplicationId { get; set; }

        [SwaggerParameter(Description = "Admin comments explaining the rejection")]
        public string? AdminComments { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        public string? ReviewedBy { get; set; }
    }

    public class RejectAgentApplicationCommandHandler : IRequestHandler<RejectAgentApplicationCommand, bool>
    {
        private readonly IAgentApplicationRepository _repository;

        public RejectAgentApplicationCommandHandler(IAgentApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(RejectAgentApplicationCommand command, CancellationToken cancellationToken)
        {
            var application = await _repository.GetByIdAsync(command.ApplicationId);
            if (application == null || application.Status != ApplicationStatus.Pending)
                return false;

            application.Status = ApplicationStatus.Rejected;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedBy = command.ReviewedBy;
            application.AdminComments = command.AdminComments;

            await _repository.UpdateAsync(application.ApplicationId, application);

            return true;
        }
    }
}
