using MediatR;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Application.Interfaces.Auth;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;


namespace IslaNova.Core.Application.Features.Agent.Commands.ChangeAgentStatus
{
    /// <summary>
    /// Command for change agent status by his unique identifier
    /// </summary>
    public class ChangeAgentStatusCommand : IRequest<Unit>
    {
        [SwaggerParameter(Description = "The unique identifier of the agent")]
        public string? Id { get; set; }

        [SwaggerParameter(Description = "The new agent status")]
        public bool Status { get; set; }
    }

    public class ChangeAgentStatusCommandHandler : IRequestHandler<ChangeAgentStatusCommand, Unit>
    {
        private readonly IAuthServiceForWebApi _authServiceForWebApi;
        public ChangeAgentStatusCommandHandler(IAuthServiceForWebApi authServiceForWebApi)
        {
            _authServiceForWebApi = authServiceForWebApi;
        }

        public async Task<Unit> Handle(ChangeAgentStatusCommand command, CancellationToken cancellationToken)
        {
            var entity = await _authServiceForWebApi.GetUserById(command.Id ?? "");

            if (entity == null)
                throw new ApiException("Error changing agent status", (int)HttpStatusCode.InternalServerError);

            await _authServiceForWebApi.ToggleUserStatus(command.Id ?? "", command.Status);
            return Unit.Value;

        }


    }
}
