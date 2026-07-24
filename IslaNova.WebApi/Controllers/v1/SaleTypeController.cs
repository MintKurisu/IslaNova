using Asp.Versioning;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.Feature;
using IslaNova.Core.Application.Dtos.PropertyManagement.SaleType;
using IslaNova.Core.Application.Features.SaleType.Commands.AddSaleType;
using IslaNova.Core.Application.Features.SaleType.Commands.DeleteSaleType;
using IslaNova.Core.Application.Features.SaleType.Commands.UpdateSaleType;
using IslaNova.Core.Application.Features.SaleType.Queries.GetAllSaleType;
using IslaNova.Core.Application.Features.SaleType.Queries.GetSaleTypeById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Provides CRUD operations for sale types.")]

    public class SaleTypeController : BaseApiController
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<SaleTypeApiDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all sale types",
            Description = "Retrieves the full list of sale types registered in the system."
        )]
        public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] string? order, [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            var result = await Mediator.Send(new GetAllSaleTypeQuery
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SaleTypeApiDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get sale type by ID",
            Description = "Retrieves detailed information for the given sale type ID."
        )]
        public async Task<IActionResult> GetById(int id)
        {
            var propertyType = await Mediator.Send(new GetSaleTypeByIdQuery() { SaleTypeId = id });

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
            Summary = "Create a new sale type",
            Description = "Adds a new sale type to the system."
        )]
        public async Task<IActionResult> Create([FromBody] AddSaleTypeCommand command)
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
            Summary = "Update an existing sale type",
            Description = "Modifies the name and description of an sale property type."
        )]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSaleTypeCommand command)
        {
            command.SaleTypeId = id;
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
            Summary = "Delete a sale type",
            Description = "Removes an existing sale type from the system."
        )]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await Mediator.Send(new DeleteSaleTypeCommand()
            {
                SaleTypeId = id
            });

            return NoContent();
        }

    }
}