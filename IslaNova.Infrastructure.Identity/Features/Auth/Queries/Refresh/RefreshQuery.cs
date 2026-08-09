using IslaNova.Core.Application.Dtos.Auth;
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

namespace IslaNova.Infrastructure.Identity.Features.Auth.Queries.Refresh
{
    public class RefreshQuery : IRequest<LoginRefreshTokenResponseDto>
    {

        [SwaggerParameter(Description = "Refresh Token")]
        public required string refreshTokenRequest { get; set; }
    }

    public class RefreshQueryHandler : IRequestHandler<RefreshQuery, LoginRefreshTokenResponseDto>
    {
        private readonly IdentityContext _context;
        private readonly UserManager<User> _userManager;
        private readonly JwtSettings _jwtSettings;

        public RefreshQueryHandler(UserManager<User> userManager, IOptions<JwtSettings> jwtSettings, IdentityContext context)
        {
            _context = context;
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginRefreshTokenResponseDto> Handle(RefreshQuery query, CancellationToken cancellationToken)
        {
            LoginRefreshTokenResponseDto response = new()
            {
                AccessToken = "",
                RefreshToken = "",
                HasError = false,
                Errors = []
            };

            RefreshToken? refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == query.refreshTokenRequest);

            if (refreshToken is null || refreshToken.Expires < DateTime.UtcNow)
            {
                response.HasError = true;
                response.Errors.Add("Invalid or expired refresh token.");
                return response;
            }

            string accesstoken = new JwtSecurityTokenHandler().WriteToken(await GenerateJwtToken(refreshToken.User!));

            refreshToken.Token = GenerateRefreshToken();
            refreshToken.Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationTime);

            await _context.SaveChangesAsync();

            response.AccessToken = accesstoken;
            response.RefreshToken = refreshToken.Token;

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