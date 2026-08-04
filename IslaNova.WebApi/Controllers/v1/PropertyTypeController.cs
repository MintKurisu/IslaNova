using Asp.Versioning;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.Feature;
using IslaNova.Core.Application.Dtos.PropertyManagement.PropertyType;
using IslaNova.Core.Application.Features.PropertyType.Commands.AddPropertyType;
using IslaNova.Core.Application.Features.PropertyType.Commands.DeletePropertyType;
using IslaNova.Core.Application.Features.PropertyType.Commands.UpdatePropertyType;
using IslaNova.Core.Application.Features.PropertyType.Queries.GetAllPropertyType;
using IslaNova.Core.Application.Features.PropertyType.Queries.GetPropertyTypeById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Provides CRUD operations for property types.")]
    public class PropertyTypeController : BaseApiController
    {
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<PropertyTypeApiDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all property types",
            Description = "Retrieves the full list of property types registered in the system."
        )]
        public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] string? order, [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            var result = await Mediator.Send(new GetAllPropertyTypeQuery
            {
                Search = search,
                Order = order,
                Page = page,
                Limit = limit
            });

            if (!result.Data.Any())
                return NoContent();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyTypeApiDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get property type by ID",
            Description = "Retrieves detailed information for the given property type ID."
        )]
        public async Task<IActionResult> GetById(int id)
        {
            var propertyType = await Mediator.Send(new GetPropertyTypeByIdQuery() { PropertyTypeId = id });

            if (propertyType == null)
            {
                return NoContent();
            }

            return Ok(propertyType);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create a new property type",
            Description = "Adds a new property type to the system."
        )]
        public async Task<IActionResult> Create([FromBody] AddPropertyTypeCommand command)
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
            Summary = "Update an existing property type",
            Description = "Modifies the name and description of an existing property type."
        )]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePropertyTypeCommand command)
        {
            command.PropertyTypeId = id;
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
            Summary = "Delete a property type",
            Description = "Removes an existing property type from the system."
        )]
        public async Task<IActionResult> Delete(int id)
        {
            await Mediator.Send(new DeletePropertyTypeCommand()
            {
                PropertyTypeId = id
            });

            return NoContent();
        }



    }
}
