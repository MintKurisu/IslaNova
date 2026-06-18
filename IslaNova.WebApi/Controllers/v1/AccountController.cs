using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IslaNova.Core.Application.Dtos.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Identity.Features.Auth.Commands.SignUp;
using IslaNova.Infrastructure.Identity.Features.Auth.Queries.Login;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Handles authentication, login, and registration of system users.")]
    public class AccountController : BaseApiController
    {

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "User Login",
            Description = "Authenticates a user and returns a valid JWT token."
        )]
        public async Task<IActionResult> Login([FromBody] LoginQuery parameters)
        {
            var response = await Mediator.Send(parameters);

            if (response == null || response.HasError)
            {
                return BadRequest(response?.Errors);
            }

            return Ok(response);

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("signUp/admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create Admin User",
            Description = "Registers a new user with Admin role. Only Admin users can access this endpoint."
        )]
        public async Task<IActionResult> SignUpAdmin([FromBody] SignUpCommand command)
        {

            command.Role = Roles.Admin.ToString();
            var result = await Mediator.Send(command);

            if (result == null || result.HasError)
            {
                return BadRequest(result?.Errors);
            }

            return Created();
        }
    }
}
