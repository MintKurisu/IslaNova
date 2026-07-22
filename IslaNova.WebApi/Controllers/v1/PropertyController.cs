using Asp.Versioning;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Features.Property.Commands.CreateProperty;
using IslaNova.Core.Application.Features.Property.Commands.DeleteProperty;
using IslaNova.Core.Application.Features.Property.Commands.UpdateProperty;
using IslaNova.Core.Application.Features.Property.Queries.FilterProperties;
using IslaNova.Core.Application.Features.Property.Queries.GetAllProperty;
using IslaNova.Core.Application.Features.Property.Queries.GetAvailableProperties;
using IslaNova.Core.Application.Features.Property.Queries.GetPropertiesByAgentId;
using IslaNova.Core.Application.Features.Property.Queries.GetPropertyByCode;
using IslaNova.Core.Application.Features.Property.Queries.GetPropertyById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Endpoints for querying and managing properties")]
    public class PropertyController : BaseApiController
    {
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<PropertyApiDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Property list", Description = "Returns all registered properties")]
        public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] string? order, [FromQuery] string? sortBy,
            [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await Mediator.Send(new GetAllPropertyQuery
            {
                Search = search,
                Order = order,
                SortBy = sortBy,
                Page = page,
                Limit = limit
            });

            if (!result.Data.Any())
                return NoContent();

            return Ok(result);
        }

        [HttpGet("available")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PropertyDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Available properties", Description = "Returns all available properties")]
        public async Task<IActionResult> GetAvailable()
        {
            var properties = await Mediator.Send(new GetAvailablePropertiesQuery());
            if (properties == null || !properties.Any())
                return NoContent();
            return Ok(properties);
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<PropertyDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Filter properties", Description = "Returns available properties filtered by type, sale type, price, bedrooms and bathrooms")]
        public async Task<IActionResult> Filter([FromQuery] FilterPropertiesQuery query)
        {
            var result = await Mediator.Send(query);

            if (!result.Data.Any())
                return NoContent();

            return Ok(result);
        }

        [HttpGet("agent/{agentId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<PropertyDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Properties by agent", Description = "Returns all properties of a specific agent")]
        public async Task<IActionResult> GetByAgentId(string agentId, [FromQuery] string? search, [FromQuery] string? order,
            [FromQuery] string? sortBy, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await Mediator.Send(new GetPropertiesByAgentIdQuery
            {
                AgentId = agentId,
                Search = search,
                Order = order,
                SortBy = sortBy,
                Page = page,
                Limit = limit
            });

            if (!result.Data.Any())
                return NoContent();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Property by Id", Description = "Get detailed information of a property by its ID")]
        public async Task<IActionResult> GetById(int id)
        {
            var property = await Mediator.Send(new GetPropertyByIdQuery() { PropertyId = id });
            if (property == null)
                return NoContent();
            return Ok(property);
        }

        [HttpGet("code/{code}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Property by Code", Description = "Get a property using its unique property code")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var property = await Mediator.Send(new GetPropertyByCodeQuery() { Code = code });
            if (property == null)
                return NoContent();
            return Ok(property);
        }

        [HttpPost]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PropertyDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Create property", Description = "Creates a new property in the system")]
        public async Task<IActionResult> Create([FromBody] CreatePropertyCommand command)
        {
            var userId = User.FindFirst("uid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { detail = "User identifier not found in token." });

            command.AgentId = userId;

            var result = await Mediator.Send(command);
            if (result == null)
                return BadRequest();

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Update property", Description = "Updates an existing property")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePropertyCommand command)
        {
            var userId = User.FindFirst("uid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { detail = "User identifier not found in token." });

            command.PropertyId = id;
            command.AgentId = userId;

            var result = await Mediator.Send(command);
            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Agent,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Delete property", Description = "Deletes a property and all its associated images and improvements")]
        public async Task<IActionResult> Delete(int id)
        {
            await Mediator.Send(new DeletePropertyCommand() { PropertyId = id });
            return NoContent();
        }
    }
}