using Asp.Versioning;
using IslaNova.Core.Application.Dtos.Offer;
using IslaNova.Core.Application.Features.Offer.Commands.AcceptOffer;
using IslaNova.Core.Application.Features.Offer.Commands.CreateOffer;
using IslaNova.Core.Application.Features.Offer.Commands.RejectOffer;
using IslaNova.Core.Application.Features.Offer.Queries.GetClientsWithOffersByPropertyId;
using IslaNova.Core.Application.Features.Offer.Queries.GetOffersByClientId;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<OfferDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get offers by property",
            Description = "Returns all offers made for a specific property")]
        public async Task<IActionResult> GetByPropertyId(int propertyId)
        {
            var offers = await Mediator.Send(new GetOffersByPropertyIdQuery() { PropertyId = propertyId });
            if (offers == null || !offers.Any())
                return NoContent();
            return Ok(offers);
        }

        [HttpGet("client/{clientId}/property/{propertyId}")]
        [Authorize(Roles = "Admin,Agent,Customer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<OfferDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get offers by client and property",
            Description = "Returns all offers made by a specific client for a specific property")]
        public async Task<IActionResult> GetByClientId(string clientId, int propertyId)
        {
            var offers = await Mediator.Send(new GetOffersByClientIdQuery() { ClientId = clientId, PropertyId = propertyId });
            if (offers == null || !offers.Any())
                return NoContent();
            return Ok(offers);
        }

        [HttpGet("property/{propertyId}/clients")]
        [Authorize(Roles = "Admin,Agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<string>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get clients with offers",
            Description = "Returns all client IDs that have made offers on a specific property")]
        public async Task<IActionResult> GetClientsWithOffers(int propertyId)
        {
            var clients = await Mediator.Send(new GetClientsWithOffersByPropertyIdQuery() { PropertyId = propertyId });
            if (clients == null || !clients.Any())
                return NoContent();
            return Ok(clients);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OfferDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create offer",
            Description = "Creates a new offer for a property. Only customers can make offers.")]
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