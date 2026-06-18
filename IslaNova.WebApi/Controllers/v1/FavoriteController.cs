using Asp.Versioning;
using IslaNova.Core.Application.Dtos.Favorite;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Features.Favorite.Commands.AddFavorite;
using IslaNova.Core.Application.Features.Favorite.Commands.RemoveFavorite;
using IslaNova.Core.Application.Features.Favorite.Queries.GetClientFavoriteProperties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Customer")]
    [SwaggerTag("Endpoints for managing client favorite properties")]
    public class FavoriteController : BaseApiController
    {
        [HttpGet("{clientId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<PropertyDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get favorite properties",
            Description = "Returns all favorite properties of a specific client.")]
        public async Task<IActionResult> GetFavorites(string clientId)
        {
            var properties = await Mediator.Send(new GetClientFavoritePropertiesQuery() { ClientId = clientId });
            if (properties == null || !properties.Any())
                return NoContent();
            return Ok(properties);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FavoriteDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Add favorite",
            Description = "Adds a property to the client's favorites.")]
        public async Task<IActionResult> Add([FromBody] AddFavoriteCommand command)
        {
            var result = await Mediator.Send(command);
            if (result == null)
                return BadRequest();
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpDelete("{clientId}/property/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Remove favorite",
            Description = "Removes a property from the client's favorites.")]
        public async Task<IActionResult> Remove(string clientId, int propertyId)
        {
            var result = await Mediator.Send(new RemoveFavoriteCommand() { ClientId = clientId, PropertyId = propertyId });
            if (!result)
                return BadRequest();
            return NoContent();
        }
    }
}