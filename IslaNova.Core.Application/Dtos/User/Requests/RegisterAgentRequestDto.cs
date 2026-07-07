using IslaNova.Core.Domain.Enums;

namespace IslaNova.Core.Application.Dtos.User.Requests
{
    public class RegisterAgentRequestDto
    {
        // Identity information
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string IdentificationNumber { get; set; }
        public required string Password { get; set; }

        // Agent application information
        public string? LicenseNumber { get; set; }
        public string? CertificationNumber { get; set; }

        public EmploymentType EmploymentType { get; set; }
        public string? AgencyName { get; set; }

        // Short professional statement for the application review
        public required string ProfessionalStatement { get; set; }
    }
}
