using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Core.Domain.Enums;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using IslaNova.Infrastructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Commands.RegisterAgent
{
    public class RegisterAgentCommand : IRequest<RegisterAgentResponseDto>
    {
        // Identity information

        [SwaggerParameter(Description = "Agent first name")]
        public string? Name { get; set; }

        [SwaggerParameter(Description = "Agent last name")]
        public string? LastName { get; set; }

        [SwaggerParameter(Description = "Unique username")]
        public string? UserName { get; set; }

        [SwaggerParameter(Description = "Email address")]
        public string? Email { get; set; }

        [SwaggerParameter(Description = "Phone number")]
        public string? PhoneNumber { get; set; }

        [SwaggerParameter(Description = "Identification number")]
        public string? IdentificationNumber { get; set; }

        [SwaggerParameter(Description = "Account password")]
        public string? Password { get; set; }

        // Agent application data

        [SwaggerParameter(Description = "Real estate license number (optional)")]
        public string? LicenseNumber { get; set; }

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

        public RegisterAgentCommandHandler(
            UserManager<User> userManager,
            IAgentApplicationRepository agentApplicationRepository)
        {
            _userManager = userManager;
            _agentApplicationRepository = agentApplicationRepository;
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

            // Email duplicate
            var existingUserByEmail = await _userManager.FindByEmailAsync(command.Email ?? "");
            if (existingUserByEmail != null)
            {
                response.HasError = true;
                response.Errors.Add("Email already exists.");
                return response;
            }

            // Username duplicate
            var existingUserByUsername = await _userManager.FindByNameAsync(command.UserName ?? "");
            if (existingUserByUsername != null)
            {
                response.HasError = true;
                response.Errors.Add("Username already exists.");
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

            var user = new User
            {
                Name = command.Name ?? "",
                LastName = command.LastName ?? "",
                UserName = command.UserName,
                Email = command.Email,
                PhoneNumber = command.PhoneNumber,
                IdentificationNumber = command.IdentificationNumber
            };

            var result = await _userManager.CreateAsync(user, command.Password ?? "");

            if (!result.Succeeded)
            {
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
            response.UserName = user.UserName ?? "";

            return response;
        }
    }
}
