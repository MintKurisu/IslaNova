using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Features.Agent.Commands.ChangeAgentStatus;
using IslaNova.Core.Application.Features.Agent.Queries.GetAgentProperty;
using IslaNova.Core.Application.Features.Agent.Queries.GetAllAgent;
using IslaNova.Core.Application.Features.Agent.Queries.GetById;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Provides management and retrieval endpoints for agents.")]
    public class AgentController : BaseApiController
    {

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AgentUserDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "List all agents",
            Description = "Returns all system agents with basic profile information."
        )]
        public async Task<IActionResult> List()
        {
            var agentList = await Mediator.Send(new GetAllAgentQuery());

            if (agentList == null || !agentList.Any())
            {
                return NoContent();
            }

            return Ok(agentList);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AgentUserDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get agent by ID",
            Description = "Returns detailed information about a specific agent.",
            OperationId = "Agent.GetById"
        )]
        public async Task<IActionResult> GetById(string id)
        {
            var agent = await Mediator.Send(new GetAgentByIdQuery() { Id = id });

            if (agent == null)
            {
                return NoContent();
            }

            return Ok(agent);
        }

        [HttpGet("{id}/properties")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<PropertyApiDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get agent properties",
            Description = "Returns all properties currently managed by the specified agent."
        )]
        public async Task<IActionResult> GetAgentProperty(string id)
        {
            var propertyList = await Mediator.Send(new GetAgentPropertyQuery() { Id = id });

            if (propertyList == null || !propertyList.Any())
            {
                return NoContent();
            }

            return Ok(propertyList);
        }


        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Change agent status",
            Description = "Allows administrators to activate or deactivate a specific agent."
        )]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] bool status)
        {
            await Mediator.Send(new ChangeAgentStatusCommand() { Id = id, Status = status });
            return NoContent();
        }

    }
}
