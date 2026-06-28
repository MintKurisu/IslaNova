using Asp.Versioning;
using IslaNova.Core.Application.Dtos.Auth;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Identity.Features.Auth.Commands.SignUp;
using IslaNova.Infrastructure.Identity.Features.Auth.Queries.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Handles authentication, login, and registration of system users.")]
    public class AccountController : BaseApiController
    {
        private readonly IAuthServiceForWebApi _authService;
        public AccountController(IAuthServiceForWebApi authService)
        {
            _authService = authService;
        }

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

        [Authorize]
        [HttpPut("updateProfile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Update profile",
            Description = "Updates the profile of the currently authenticated user."
        )]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { detail = "User identifier not found in token." });

            var currentUser = await _authService.GetUserById(userId);
            if (currentUser == null)
                return NotFound(new { detail = "User not found." });

            var updateDto = new UpdateUserDto
            {
                Id = userId,
                Name = dto.Name ?? "",
                LastName = dto.LastName ?? "",
                UserName = dto.UserName ?? "",
                Email = dto.Email ?? "",
                PhoneNumber = dto.PhoneNumber ?? "",
                ProfileImage = dto.ProfileImage,
                Password = dto.Password ?? "",
                IdentificationNumber = currentUser.IdentificationNumber 
            };

            var result = await _authService.UpdateUserAsync(updateDto, isCreated: false);

            if (result.HasError)
                return BadRequest(result.Errors);

            return Ok(result);
        }
    }
}
