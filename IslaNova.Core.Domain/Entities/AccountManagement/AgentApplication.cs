using IslaNova.Core.Domain.Enums;

namespace IslaNova.Core.Domain.Entities.AccountManagement
{
    public class AgentApplication
    {
        public int ApplicationId { get; set; }

        // User who submitted the application
        public required string UserId { get; set; }

        // Professional information
        public string? LicenseNumber { get; set; }
        public string? CertificationNumber { get; set; }

        // Employment information
        public EmploymentType EmploymentType { get; set; }
        public string? AgencyName { get; set; }

        // Short professional statement used during the review process
        public required string ProfessionalStatement { get; set; }

        // Application review information
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public string? AdminComments { get; set; }
    }
}
