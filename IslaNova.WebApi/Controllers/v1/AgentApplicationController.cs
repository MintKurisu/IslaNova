using IslaNova.Core.Application.Dtos.AgentApplication;
using IslaNova.Core.Application.Features.AgentApplication.Commands.ApproveAgentApplication;
using IslaNova.Core.Application.Features.AgentApplication.Commands.RejectAgentApplication;
using IslaNova.Core.Application.Features.AgentApplication.Queries.GetAgentApplicationById;
using IslaNova.Core.Application.Features.AgentApplication.Queries.GetAllAgentApplications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AgentApplicationController : BaseApiController
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<AgentApplicationDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all agent applications",
            Description = "Returns all agent applications. Only admins can access this endpoint.")]
        public async Task<IActionResult> List()
        {
            var applications = await Mediator.Send(new GetAllAgentApplicationsQuery());
            if (applications == null || !applications.Any())
                return NoContent();
            return Ok(applications);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AgentApplicationDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get agent application by ID",
            Description = "Returns a specific agent application by its ID.")]
        public async Task<IActionResult> GetById(int id)
        {
            var application = await Mediator.Send(new GetAgentApplicationByIdQuery { ApplicationId = id });
            if (application == null)
                return NotFound();
            return Ok(application);
        }

        [HttpPut("{id}/approve")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Approve agent application",
            Description = "Approves an agent application and activates the agent account.")]
        public async Task<IActionResult> Approve(int id)
        {
            var adminId = User.FindFirst("uid")?.Value;

            var result = await Mediator.Send(new ApproveAgentApplicationCommand
            {
                ApplicationId = id,
                ReviewedBy = adminId
            });

            if (!result)
                return BadRequest();

            return NoContent();
        }

        [HttpPut("{id}/reject")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Reject agent application",
            Description = "Rejects an agent application with optional admin comments.")]
        public async Task<IActionResult> Reject(int id, [FromBody] string? adminComments)
        {
            var adminId = User.FindFirst("uid")?.Value;

            var result = await Mediator.Send(new RejectAgentApplicationCommand
            {
                ApplicationId = id,
                AdminComments = adminComments,
                ReviewedBy = adminId
            });

            if (!result)
                return BadRequest();

            return NoContent();
        }
    }
}
