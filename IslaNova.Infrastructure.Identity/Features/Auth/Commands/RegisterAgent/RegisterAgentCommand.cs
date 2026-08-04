using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Interfaces.Storage;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Core.Domain.Enums;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using IslaNova.Infrastructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Commands.RegisterAgent
{
    public class RegisterAgentCommand : IRequest<RegisterAgentResponseDto>
    {
        // Identity information

        [SwaggerParameter(Description = "Agent first name")]
        public required string Name { get; set; }

        [SwaggerParameter(Description = "Agent last name")]
        public required string LastName { get; set; }

        [SwaggerParameter(Description = "Email address")]
        public required string Email { get; set; }


        [SwaggerParameter(Description = "Phone number")]
        public string? PhoneNumber { get; set; }

        [SwaggerParameter(Description = "Identification number")]
        public required string IdentificationNumber { get; set; }

        [SwaggerParameter(Description = "Agent Profile Image")]
        public IFormFile? ProfileImageFile { get; set; }

        [SwaggerParameter(Description = "Account password")]
        public required string Password { get; set; }

        // Agent application data

        [SwaggerParameter(Description = "Real estate license number (optional)")]
        public required string LicenseNumber { get; set; }

        [SwaggerParameter(Description = "Professional certification number (optional)")]
        public string? CertificationNumber { get; set; }

        [SwaggerParameter(Description = "Employment type (Independent or Agency)")]
        public EmploymentType EmploymentType { get; set; }

        [SwaggerParameter(Description = "Agency name if applies")]
        public string? AgencyName { get; set; }

        [SwaggerParameter(Description = "Professional statement / motivation letter")]
        public string? ProfessionalStatement { get; set; }
    }
    public class RegisterAgentCommandHandler : IRequestHandler<RegisterAgentCommand, RegisterAgentResponseDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IAgentApplicationRepository _agentApplicationRepository;
        private readonly IStorageService _storageService;

        public RegisterAgentCommandHandler(
            UserManager<User> userManager,
            IAgentApplicationRepository agentApplicationRepository,
            IStorageService storageService)
        {
            _userManager = userManager;
            _agentApplicationRepository = agentApplicationRepository;
            _storageService = storageService;
        }

        public async Task<RegisterAgentResponseDto> Handle(RegisterAgentCommand command, CancellationToken cancellationToken)
        {
            var response = new RegisterAgentResponseDto
            {
                UserId = "",
                Email = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            string? imageUrl = null;

            // Email duplicate
            var existingUserByEmail = await _userManager.FindByEmailAsync(command.Email ?? "");
            if (existingUserByEmail != null)
            {
                response.HasError = true;
                response.Errors.Add("Email already exists.");
                return response;
            }

            // IdentificationNumber duplicate
            var existingUserByIdNumber = _userManager.Users
                .FirstOrDefault(u => u.IdentificationNumber == command.IdentificationNumber);

            if (existingUserByIdNumber != null)
            {
                response.HasError = true;
                response.Errors.Add("Identification number already exists.");
                return response;
            }

            if (command.ProfileImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString();
                imageUrl = await _storageService.UploadAsync(command.ProfileImageFile, "profile-images", "agents", fileName);
            }


            var user = new User
            {
                Name = command.Name ?? "",
                LastName = command.LastName ?? "",
                Email = command.Email,
                UserName = command.Email,
                PhoneNumber = command.PhoneNumber,
                IdentificationNumber = command.IdentificationNumber,
                ProfileImage = imageUrl


            };

            var result = await _userManager.CreateAsync(user, command.Password ?? "");

            if (!result.Succeeded)
            {
                if (imageUrl != null)
                {
                    await _storageService.DeleteAsync(imageUrl, "profile-images");
                }
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(e => e.Description));
                return response;
            }

            await _userManager.AddToRoleAsync(user, Roles.Agent.ToString());

            var existingApplication = await _agentApplicationRepository
                .GetByUserIdAsync(user.Id);

            if (existingApplication != null)
            {
                await _userManager.DeleteAsync(user);

                response.HasError = true;
                response.Errors.Add("This user already has an agent application.");
                return response;
            }

            var application = new AgentApplication
            {
                UserId = user.Id,
                LicenseNumber = command.LicenseNumber,
                CertificationNumber = command.CertificationNumber,
                EmploymentType = command.EmploymentType,
                AgencyName = command.AgencyName,
                ProfessionalStatement = command.ProfessionalStatement,
                Status = ApplicationStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            var createdApplication = await _agentApplicationRepository.AddAsync(application);


            if (createdApplication == null)
            {
                await _userManager.DeleteAsync(user);

                response.HasError = true;
                response.Errors.Add("Failed to create agent application. Registration rolled back.");
                return response;
            }

            response.UserId = user.Id;
            response.Email = user.Email ?? "";

            return response;
        }
    }
}
