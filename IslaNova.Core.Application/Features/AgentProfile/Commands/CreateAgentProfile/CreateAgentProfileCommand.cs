using AutoMapper;
using IslaNova.Core.Application.Dtos.AgentProfile;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace IslaNova.Core.Application.Features.AgentProfile.Commands.CreateAgentProfile
{
    public class CreateAgentProfileCommand : IRequest<AgentProfileDto?>
    {
        [JsonIgnore]
        public string? AgentId { get; set; }
        [SwaggerParameter(Description = "Agent bio")]
        public string? Bio { get; set; }
        [SwaggerParameter(Description = "Years of experience")]
        public int? YearsOfExperience { get; set; }
        [SwaggerParameter(Description = "WhatsApp number")]
        public string? WhatsappNumber { get; set; }
        [SwaggerParameter(Description = "Facebook URL")]
        public string? FacebookUrl { get; set; }
        [SwaggerParameter(Description = "Instagram URL")]
        public string? InstagramUrl { get; set; }
        [SwaggerParameter(Description = "Specialty zones")]
        public string? SpecialtyZones { get; set; }
    }

    public class CreateAgentProfileCommandHandler : IRequestHandler<CreateAgentProfileCommand, AgentProfileDto?>
    {
        private readonly IAgentProfileRepository _agentProfileRepository;
        private readonly IMapper _mapper;

        public CreateAgentProfileCommandHandler(
            IAgentProfileRepository agentProfileRepository,
            IMapper mapper)
        {
            _agentProfileRepository = agentProfileRepository;
            _mapper = mapper;
        }

        public async Task<AgentProfileDto?> Handle(CreateAgentProfileCommand command, CancellationToken cancellationToken)
        {
            var existingProfile = await _agentProfileRepository.GetByAgentIdAsync(command.AgentId ?? "");
            if (existingProfile != null) return null;

            var profile = new Domain.Entities.AccountManagement.AgentProfile
            {
                AgentId = command.AgentId ?? "",
                Bio = command.Bio,
                YearsOfExperience = command.YearsOfExperience,
                WhatsappNumber = command.WhatsappNumber,
                FacebookUrl = command.FacebookUrl,
                InstagramUrl = command.InstagramUrl,
                SpecialtyZones = command.SpecialtyZones
            };

            var created = await _agentProfileRepository.AddAsync(profile);
            return _mapper.Map<AgentProfileDto>(created);
        }
    }
}
