using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IslaNova.Core.Application.Dtos.Auth;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Interfaces.Email;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Settings;
using IslaNova.Infrastructure.Identity.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IslaNova.Infrastructure.Identity.Service
{
    public class AuthServiceForWebApi : BaseAuthService, IAuthServiceForWebApi
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtSettings _jwtSettings;


        public AuthServiceForWebApi(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<JwtSettings> jwtSettings,
            IEmailService emailService
           ) : base(userManager, emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginResponseForApiDto> LoginAsync(LoginDto loginDto)
        {
            LoginResponseForApiDto response = new()
            {
                Name = "",
                LastName = "",
                HasError = false,
                Errors = []

            };

            var user = await _userManager.FindByNameAsync(loginDto.Identifier);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There's no account registered with this username: {loginDto.Identifier}");
                return response;

            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Errors.Add($"This account for user {loginDto.Identifier} in not active, please check your email");
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.Add($"This credentials are invalid for this user: {loginDto.Identifier}");
                return response;
            }


            JwtSecurityToken jwtSecurityToken = await GenerateJwtToken(user);

            response.Name = user.Name;
            response.LastName = user.LastName;
            response.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return response;
        }

        public async Task<SignUpResponseDto> SignUpAsync(SignUpDto dto)
        {
            SignUpResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                IdentificationNumber = "",
                UserName = "",
                PhoneNumber = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(dto.UserName);

            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"Username {dto.UserName} is already taken.");
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"Email {dto.Email} is already taken.");
                return response;
            }

            var userWithSameIdentificationNumber = await _userManager.Users.FirstOrDefaultAsync(w => w.IdentificationNumber == dto.IdentificationNumber);

            if (userWithSameIdentificationNumber != null)
            {
                response.HasError = true;
                response.Errors.Add($"this identification number: {dto.IdentificationNumber} is already taken.");
                return response;
            }

            if (dto.Role == Roles.Customer.ToString() || dto.Role == Roles.Agent.ToString())
            {
                response.HasError = true;
                response.Errors.Add($"You cannot create user with {dto.Role} role.");
                return response;
            }


            User user = new()
            {
                Name = dto.Name,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.UserName,
                IdentificationNumber = dto.IdentificationNumber,
                PhoneNumber = dto.PhoneNumber,
                ProfileImage = dto.ProfileImage,
                EmailConfirmed = true, // Admin users are created active
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, dto.Role);

                var rolesList = await _userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.IsVerified = user.EmailConfirmed;
                response.PhoneNumber = user.PhoneNumber;
                response.Roles = rolesList.ToList();

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }

        public override async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false)
        {
            return await base.ForgotPasswordAsync(request, isApi);
        }

        public async Task<ForgotPasswordResponseDto> ResetPasswordApiAsync(ResetPasswordApiRequestDto request)
        {
            ForgotPasswordResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no account registered with this user");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

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
