using IslaNova.Core.Application.Dtos.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Settings;
using IslaNova.Infrastructure.Identity.Contexts;
using IslaNova.Infrastructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Queries.Login
{
    /// <summary>
    /// Query used to authenticate a user using their email, 
    /// along with their password.
    /// </summary>
    public class LoginQuery : IRequest<LoginResponseForApiDto>
    {
        /// <example>john.doe@example.com</example>
        [SwaggerParameter(Description = "Identifier of the user to login. using Email")]
        public string? Identifier { get; set; }
        public string? Password { get; set; }
    }
    public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResponseForApiDto>
    {
        private readonly IdentityContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtSettings _jwtSettings;

        public LoginQueryHandler(UserManager<User> userManager, SignInManager<User> signInManager, IOptions<JwtSettings> jwtSettings, IdentityContext context)
        {
            _context = context;
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

            var user = await _userManager.FindByEmailAsync(query.Identifier ?? "");

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There's no account registered with this email: {query.Identifier ?? ""}");
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
                response.Errors.Add($"This credentials are invalid for this email: {query.Identifier}");
                return response;
            }


            JwtSecurityToken jwtSecurityToken = await GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            var existing = await _context.RefreshTokens
              .FirstOrDefaultAsync(rt => rt.UserId == user.Id);


            if (existing != null)
            {
                // UPDATE existing refresh token 
                existing.Token = newRefreshToken;
                existing.Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationTime);
            }
            else
            {
                RefreshToken refreshToken = new()
                {
                    Id = Guid.NewGuid(),
                    Token = newRefreshToken,
                    Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationTime),
                    UserId = user.Id
                };

                _context.RefreshTokens.Add(refreshToken);
            }

            await _context.SaveChangesAsync();



            var roleString = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            if (!Enum.TryParse<Roles>(roleString, out var role))
                throw new Exception($"Invalid role '{roleString}'");

            response.Name = user.Name;
            response.LastName = user.LastName;
            response.Role = role;
            response.ProfileImage = user.ProfileImage ?? "";
            response.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.RefreshToken = newRefreshToken;

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
        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        #endregion
    }
}
