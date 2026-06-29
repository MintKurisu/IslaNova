using Asp.Versioning;
using IslaNova.Core.Application.Dtos.AgentProfile;
using IslaNova.Core.Application.Features.AgentProfile.Commands.CreateAgentProfile;
using IslaNova.Core.Application.Features.AgentProfile.Commands.DeleteAgentProfile;
using IslaNova.Core.Application.Features.AgentProfile.Commands.UpdateAgentProfile;
using IslaNova.Core.Application.Features.AgentProfile.Queries.GetAgentProfileByAgentId;
using IslaNova.Core.Application.Features.AgentProfile.Queries.GetAllAgentProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize]
    [SwaggerTag("Endpoints for managing agent profiles")]
    public class AgentProfileController : BaseApiController
    {
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<AgentProfileDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all agent profiles",
            Description = "Returns all agent profiles registered in the system.")]
        public async Task<IActionResult> List()
        {
            var profiles = await Mediator.Send(new GetAllAgentProfilesQuery());
            if (profiles == null || !profiles.Any())
                return NoContent();
            return Ok(profiles);
        }

        [HttpGet("{agentId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AgentProfileDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get agent profile by agent ID",
            Description = "Returns the profile of a specific agent.")]
        public async Task<IActionResult> GetByAgentId(string agentId)
        {
            var profile = await Mediator.Send(new GetAgentProfileByAgentIdQuery() { AgentId = agentId });
            if (profile == null)
                return NoContent();
            return Ok(profile);
        }

        [HttpPost]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AgentProfileDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create agent profile",
            Description = "Creates a new profile for an agent.")]
        [HttpPost]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> Create([FromBody] CreateAgentProfileCommand command)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { detail = "User identifier not found in token." });

            command.AgentId = userId;

            var result = await Mediator.Send(command);
            if (result == null)
                return BadRequest();
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> Update([FromBody] UpdateAgentProfileCommand command)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { detail = "User identifier not found in token." });

            command.AgentId = userId;

            var result = await Mediator.Send(command);
            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        [HttpDelete]
        [Authorize(Roles = "Agent,Admin")]
        public async Task<IActionResult> Delete()
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { detail = "User identifier not found in token." });

            var result = await Mediator.Send(new DeleteAgentProfileCommand() { AgentId = userId });
            if (!result)
                return BadRequest();
            return NoContent();
        }
    }
}
