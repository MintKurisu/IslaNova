namespace IslaNova.Core.Application.Dtos.AgentApplication
{
    public class AgentApplicationDto
    {
        public int ApplicationId { get; set; }
        public required string UserId { get; set; }
        public string? LicenseNumber { get; set; }
        public string? CertificationNumber { get; set; }
        public string EmploymentType { get; set; } = "";
        public string? AgencyName { get; set; }
        public required string ProfessionalStatement { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public string? AdminComments { get; set; }

        // Agent info from Identity
        public string? AgentName { get; set; }
        public string? AgentEmail { get; set; }
        public string? AgentPhone { get; set; }
        public string? AgentProfileImage { get; set; }
    }
}
