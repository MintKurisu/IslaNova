using Asp.Versioning;
using IslaNova.Core.Application.Common.Models;
using IslaNova.Core.Application.Dtos.Auth;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Settings;
using IslaNova.Infrastructure.Identity.Features.Auth.Commands.RegisterAgent;
using IslaNova.Infrastructure.Identity.Features.Auth.Commands.RevokeRefreshToken;
using IslaNova.Infrastructure.Identity.Features.Auth.Commands.SignUp;
using IslaNova.Infrastructure.Identity.Features.Auth.Queries.GetAllUsers;
using IslaNova.Infrastructure.Identity.Features.Auth.Queries.GetUserById;
using IslaNova.Infrastructure.Identity.Features.Auth.Queries.Login;
using IslaNova.Infrastructure.Identity.Features.Auth.Queries.Refresh;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Handles authentication, login, and registration of system users.")]
    public class AccountController : BaseApiController
    {
        private readonly IAuthServiceForWebApi _authService;
        private readonly JwtSettings _jwtSettings;
        public AccountController(IAuthServiceForWebApi authService, IOptions<JwtSettings> jwtSettings)
        {
            _authService = authService;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
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

            SetAuthCookies(response.AccessToken!, response.RefreshToken!);

            // Don't send tokens in the body 
            return Ok(new UserDto()
            {
                Id = response.User.Id,
                Name = response.User.Name,
                LastName = response.User.LastName,
                Email = response.User.Email ?? "",
                IdentificationNumber = response.User.IdentificationNumber,
                PhoneNumber = response.User.PhoneNumber ?? "",
                Role = response.User.Role,
                UserName = response.User.UserName ?? "",
                ProfileImage = response.User.ProfileImage ?? ""
            });

        }


        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginRefreshTokenResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "User Login with refresh token",
            Description = "Refresh authentiation for a user and returns a valid JWT token."
        )]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            RefreshQuery refreshTokenRequest = new() { refreshTokenRequest = refreshToken };

            var result = await Mediator.Send(refreshTokenRequest);

            SetAuthCookies(result.AccessToken, result.RefreshToken);

            return Ok(); // frontend doesn't need the token, it's in the cookie
        }



        [HttpGet("me")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.Agent)}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser()
        {
            var currentUserId = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            GetUserByIdQuery getUserByIdQuery = new() { UserId = currentUserId };

            var user = await Mediator.Send(getUserByIdQuery);

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }



        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
           Summary = "List all users",
           Description = "Returns all system users "
       )]
        public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] string? order, [FromQuery] int page = 1,
           [FromQuery] int limit = 10)
        {
            var result = await Mediator.Send(new GetAllUsersQuery
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



        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            RevokeRefreshTokenCommand refreshTokenRequest = new() { UserId = userId };
            await Mediator.Send(refreshTokenRequest);

            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return Ok();
        }



        [Authorize(Roles = "Admin")]
        [HttpPost("signUp/admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create Admin User",
            Description = "Registers a new user with Admin role. Only Admin users can access this endpoint."
        )]
        public async Task<IActionResult> SignUpAdmin([FromForm] SignUpCommand command)
        {

            command.Role = Roles.Admin.ToString();
            var result = await Mediator.Send(command);

            if (result == null || result.HasError)
            {
                return BadRequest(result?.Errors);
            }

            return Created();
        }

        [AllowAnonymous]
        [HttpPost("signUp/agent")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
        Summary = "Register Agent",
        Description = "Registers a new agent with their professional application."
        )]
        public async Task<IActionResult> SignUpAgent([FromForm] RegisterAgentCommand command)
        {
            var result = await Mediator.Send(command);
            if (result == null || result.HasError)
                return BadRequest(result?.Errors);
            return Created();
        }

        [Authorize]
        [Consumes("multipart/form-data")]
        [HttpPut("updateProfile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Update profile",
            Description = "Updates the profile of the currently authenticated user."
        )]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
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
                Email = dto.Email ?? currentUser.Email,
                PhoneNumber = dto.PhoneNumber ?? "",
                ProfileImageFile = dto.ProfileImageFile,
                Password = dto.Password ?? "",
                IdentificationNumber = currentUser.IdentificationNumber
            };

            var result = await _authService.UpdateUserAsync(updateDto, isCreated: false);

            if (result.HasError)
                return BadRequest(result.Errors);

            return Ok(result);
        }



        #region Private Methods
        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,          // required if SameSite=None (needed for cross-site / cross-port in dev)
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes) // match your JWT expiry
            };

            Response.Cookies.Append("accessToken", accessToken, cookieOptions);

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationTime),
                Path = "/api/auth" // restrict where it's sent, tighter security
            });
        }
        #endregion
    }
}
