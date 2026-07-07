namespace IslaNova.Core.Application.Dtos.AgentProfile
{
    public class AgentProfileDto
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
