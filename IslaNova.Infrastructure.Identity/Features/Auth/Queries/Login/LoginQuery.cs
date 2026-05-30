using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IslaNova.Core.Application.Dtos.Auth;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Domain.Settings;
using IslaNova.Infrastructure.Identity.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Queries.Login
{
    /// <summary>
    /// Query used to authenticate a user using either their username, email, 
    /// along with their password.
    /// </summary>
    public class LoginQuery : IRequest<LoginResponseForApiDto>
    {
        /// <example>john.doe@example.com</example>
        [SwaggerParameter(Description = "Identifier of the user to login. Either Email or Username")]
        public string? Identifier { get; set; }
        public string? Password { get; set; }
    }
    public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResponseForApiDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtSettings _jwtSettings;

        public LoginQueryHandler(UserManager<User> userManager, SignInManager<User> signInManager, IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginResponseForApiDto> Handle(LoginQuery query, CancellationToken cancellationToken)
        {
            LoginResponseForApiDto response = new()
            {
                Name = "",
                LastName = "",
                HasError = false,
                Errors = []

            };

            var user = await _userManager.FindByNameAsync(query.Identifier ?? "");

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There's no account registered with this username: {query.Identifier ?? ""}");
                return response;

            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Errors.Add($"This account for user {query.Identifier} in not active, please check your email");
                return response;
            }


            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", query.Password ?? "", false, true);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.Add($"This credentials are invalid for this user: {query.Identifier}");
                return response;
            }


            var userRole = await _userManager.GetRolesAsync(user);

            JwtSecurityToken jwtSecurityToken = await GenerateJwtToken(user);

            response.Name = user.Name;
            response.LastName = user.LastName;
            response.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return response;
        }

        #region "Private methods"

        private async Task<JwtSecurityToken> GenerateJwtToken(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var rolesClaims = new List<Claim>();
            foreach (var role in roles)
            {
                rolesClaims.Add(new Claim("roles", role));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("uid",user.Id ?? "")
            }.Union(userClaims).Union(rolesClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials
            );

            return jwtSecurityToken;
        }

        #endregion
    }
}
