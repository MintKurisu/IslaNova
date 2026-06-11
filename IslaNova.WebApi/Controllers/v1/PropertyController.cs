using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Features.Property.Queries.GetAllProperty;
using IslaNova.Core.Application.Features.Property.Queries.GetPropertyByCode;
using IslaNova.Core.Application.Features.Property.Queries.GetPropertyById;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;


namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Endpoints for querying properties")]
    public class PropertyController : BaseApiController
    {

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PropertyDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Property list",
            Description = "Returns all registered properties"
        )]
        public async Task<IActionResult> List()
        {
            var properties = await Mediator.Send(new GetAllPropertyQuery());

            if (properties == null || !properties.Any())
            {
                return NoContent();
            }

            return Ok(properties);
        }

        [HttpGet("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Property by Id",
            Description = "Get detailed information of a property by its ID"
        )]
        public async Task<IActionResult> GetById(int id)
        {
            var property = await Mediator.Send(new GetPropertyByIdQuery() { PropertyId = id });

            if (property == null)
            {
                return NoContent();
            }

            return Ok(property);
        }


        [HttpGet("code/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Property by Code",
            Description = "Get a property using its unique property code"
        )]
        public async Task<IActionResult> GetByCode(string code)
        {
            var property = await Mediator.Send(new GetPropertyByCodeQuery() { Code = code });

            if (property == null)
            {
                return NoContent();
            }

            return Ok(property);
        }

    }
}
