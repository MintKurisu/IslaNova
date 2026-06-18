using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IslaNova.Core.Application.Dtos.Feature;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Features.Improvement.Commands.AddImprovement;
using IslaNova.Core.Application.Features.Improvement.Commands.DeleteImprovement;
using IslaNova.Core.Application.Features.Improvement.Commands.UpdateImprovement;
using IslaNova.Core.Application.Features.Improvement.Queries.GetAllImprovement;
using IslaNova.Core.Application.Features.Improvement.Queries.GetImprovementById;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Provides CRUD operations for properties.")]
    public class ImprovementController : BaseApiController
    {

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AgentUserDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "List all improvements",
            Description = "Returns a list of all registered property improvements."
        )]
        public async Task<IActionResult> List()
        {
            var improvementList = await Mediator.Send(new GetAllImprovementQuery());

            if (improvementList == null || !improvementList.Any())
            {
                return NoContent();
            }

            return Ok(improvementList);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AgentUserDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get improvement by ID",
            Description = "Returns detailed information about a specific improvement."
        )]
        public async Task<IActionResult> GetById(int id)
        {
            var improvement = await Mediator.Send(new GetImprovementByIdQuery() { ImprovementId = id });

            if (improvement == null)
            {
                return NoContent();
            }

            return Ok(improvement);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create improvement",
            Description = "Adds a new property improvement to the system. Only admins can perform this action."
        )]
        public async Task<IActionResult> Create([FromBody] AddImprovementCommand command)
        {
            await Mediator.Send(command);
            return Created();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ImprovementDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Update improvement",
            Description = "Allows administrators to update an existing improvement."
        )]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateImprovementCommand command)
        {
            command.ImprovementId = id;
            var response = await Mediator.Send(command);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Delete improvement",
            Description = "Removes an improvement from the system. Only admins can delete improvements."
        )]
        public async Task<IActionResult> Delete(int id)
        {
            await Mediator.Send(new DeleteImprovementCommand()
            {
                ImprovementId = id
            });

            return NoContent();
        }

    }
}
