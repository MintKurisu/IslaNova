using Asp.Versioning;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.Offer;
using IslaNova.Core.Application.Features.Offer.Commands.AcceptOffer;
using IslaNova.Core.Application.Features.Offer.Commands.CreateOffer;
using IslaNova.Core.Application.Features.Offer.Commands.RejectOffer;
using IslaNova.Core.Application.Features.Offer.Queries.GetOffersByPropertyId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize]
    [SwaggerTag("Endpoints for managing property offers")]
    public class OfferController : BaseApiController
    {
        [HttpGet("property/{propertyId}")]
        [Authorize(Roles = "Admin,Agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<OfferDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get offers by property",
            Description = "Returns all offers made for a specific property"
        )]
        public async Task<IActionResult> GetByPropertyId(int propertyId,[FromQuery] string? order,[FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            var result = await Mediator.Send(new GetOffersByPropertyIdQuery
            {
                PropertyId = propertyId,
                Order = order,
                Page = page,
                Limit = limit
            });

            if (!result.Data.Any())
                return NoContent();

            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OfferDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create offer",
            Description = "Creates a new offer for a property.")]
        public async Task<IActionResult> Create([FromBody] CreateOfferCommand command)
        {
            var result = await Mediator.Send(command);
            if (result == null)
                return BadRequest();
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{offerId}/accept")]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Accept offer",
            Description = "Accepts an offer, rejects all other pending offers for the property and marks it as sold.")]
        public async Task<IActionResult> Accept(int offerId, [FromQuery] int propertyId) 
        {
            var result = await Mediator.Send(new AcceptOfferCommand() { OfferId = offerId, PropertyId = propertyId });
            if (!result)
                return BadRequest();
            return NoContent();
        }

        [HttpPut("{offerId}/reject")]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Reject offer",
            Description = "Rejects a pending offer for a property.")]
        public async Task<IActionResult> Reject(int offerId)
        {
            var result = await Mediator.Send(new RejectOfferCommand() { OfferId = offerId });
            if (!result)
                return BadRequest();
            return NoContent();
        }
    }
}