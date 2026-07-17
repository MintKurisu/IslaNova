using Asp.Versioning;
using IslaNova.Core.Application.Features.Storage.Commands.UploadProfileImage;
using IslaNova.Core.Application.Features.Storage.Commands.UploadPropertyImage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize]
    [SwaggerTag("Handles image uploads to Supabase cloud storage.")]
    public class StorageController : BaseApiController
    {
        [HttpPost("property-image")]
        [Authorize(Roles = "Agent")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Upload Property Image",
            Description = "Uploads an image to Supabase storage and returns its public URL. Only agents can upload property images."
        )]
        public async Task<IActionResult> UploadPropertyImage(IFormFile file)
        {
            var url = await Mediator.Send(new UploadPropertyImageCommand { File = file });
            if (url == null) return StatusCode(500);
            return Ok(new { url });
        }

        [HttpPost("profile-image")]
        [Authorize(Roles = "Agent")] // !
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Upload Profile Image",
            Description = "Uploads a profile image to Supabase storage and returns its public URL."
        )]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            var url = await Mediator.Send(new UploadProfileImageCommand { File = file });
            if (url == null) return StatusCode(500);
            return Ok(new { url });
        }
    }
}
