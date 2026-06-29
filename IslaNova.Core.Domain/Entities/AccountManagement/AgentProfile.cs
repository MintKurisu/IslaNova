namespace IslaNova.Core.Domain.Entities.AccountManagement
{
    public class AgentProfile
    {
        public int AgentProfileId { get; set; }
        public required string AgentId { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? WhatsappNumber { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? SpecialtyZones { get; set; }
    }
}
