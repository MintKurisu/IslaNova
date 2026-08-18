using IslaNova.Core.Application.Dtos.Auth;
using IslaNova.Core.Application.Interfaces.Storage;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Commands.SignUp
{
    /// <summary>
    /// Command used to register a new user in the system.
    /// </summary>
    public class SignUpCommand : IRequest<SignUpResponseDto>
    {
        /// <example>John</example>
        [SwaggerParameter(Description = "Name of the user to create.")]
        public string? Name { get; set; }

        /// <example>Doe</example>
        [SwaggerParameter(Description = "LastName of the user to create.")]
        public string? LastName { get; set; }

        /// <example>00234567891</example>
        [SwaggerParameter(Description = "IdentificationNumber of the user to create.")]
        public string? IdentificationNumber { get; set; }

        /// <example>john.doe@example.com</example>
        [SwaggerParameter(Description = "Email of the user to create.")]
        public string? Email { get; set; }

        [SwaggerParameter(Description = "Agent Profile Image")]
        public IFormFile? ProfileImageFile { get; set; }

        [SwaggerParameter(Description = "Account password")]
        public required string Password { get; set; }

        /// <example>8095551234</example>
        [SwaggerParameter(Description = "PhoneNumber of the user to create.")]
        public string? PhoneNumber { get; set; }


        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public string? Role { get; set; }
    }


    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, SignUpResponseDto>
    {

        private readonly UserManager<User> _userManager;
        private readonly IStorageService _storageService;

        public SignUpCommandHandler(UserManager<User> userManager, IStorageService storageService)
        {
            _userManager = userManager;
            _storageService = storageService;
        }

        public async Task<SignUpResponseDto> Handle(SignUpCommand command, CancellationToken cancellationToken)
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

            string? imageUrl = null;


            var userWithSameEmail = await _userManager.FindByEmailAsync(command.Email ?? "");
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"Email {command.Email} is already taken.");
                return response;
            }

            var userWithSameIdentificationNumber = await Task.FromResult(
                    _userManager.Users?.AsEnumerable()
                    .FirstOrDefault(w => w.IdentificationNumber == command.IdentificationNumber)
);


            if (userWithSameIdentificationNumber != null)
            {
                response.HasError = true;
                response.Errors.Add($"this identification number: {command.IdentificationNumber} is already taken.");
                return response;
            }

            if (command.Role != Roles.Admin.ToString())
            {
                response.HasError = true;
                response.Errors.Add($"Invalid Role.");
                return response;
            }

            if (command.ProfileImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString();
                imageUrl = await _storageService.UploadAsync(command.ProfileImageFile, "profile-images", "admins", fileName);
            }

            User user = new()
            {
                Name = command.Name ?? "",
                LastName = command.LastName ?? "",
                Email = command.Email,
                UserName = command.Email,
                IdentificationNumber = command.IdentificationNumber ?? "",
                PhoneNumber = command.PhoneNumber,
                EmailConfirmed = command.Role == Roles.Admin.ToString(),
                ProfileImage = imageUrl
            };

            var result = await _userManager.CreateAsync(user, command.Password ?? "");




            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, command.Role ?? "");

                var rolesList = await _userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.IsVerified = user.EmailConfirmed;
                response.PhoneNumber = user.PhoneNumber ?? "";
                response.Roles = rolesList.ToList();
                response.ProfileImage = user.ProfileImage;

                return response;
            }
            else
            {
                if (imageUrl != null)
                {
                    await _storageService.DeleteAsync(imageUrl, "profile-images");
                }

                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }
    }
}
