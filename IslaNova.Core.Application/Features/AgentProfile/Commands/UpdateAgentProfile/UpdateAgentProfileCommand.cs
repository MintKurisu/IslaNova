using AutoMapper;
using IslaNova.Core.Application.Dtos.AgentProfile;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace IslaNova.Core.Application.Features.AgentProfile.Commands.UpdateAgentProfile
{
    public class UpdateAgentProfileCommand : IRequest<AgentProfileDto?>
    {
        [JsonIgnore]
        public string? AgentId { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? WhatsappNumber { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? SpecialtyZones { get; set; }
    }

    public class UpdateAgentProfileCommandHandler : IRequestHandler<UpdateAgentProfileCommand, AgentProfileDto?>
    {
        private readonly IAgentProfileRepository _agentProfileRepository;
        private readonly IMapper _mapper;

        public UpdateAgentProfileCommandHandler(
            IAgentProfileRepository agentProfileRepository,
            IMapper mapper)
        {
            _agentProfileRepository = agentProfileRepository;
            _mapper = mapper;
        }

        public async Task<AgentProfileDto?> Handle(UpdateAgentProfileCommand command, CancellationToken cancellationToken)
        {
            var profile = await _agentProfileRepository.GetByAgentIdAsync(command.AgentId ?? "");
            if (profile == null) return null;

            profile.Bio = command.Bio;
            profile.YearsOfExperience = command.YearsOfExperience;
            profile.WhatsappNumber = command.WhatsappNumber;
            profile.FacebookUrl = command.FacebookUrl;
            profile.InstagramUrl = command.InstagramUrl;
            profile.SpecialtyZones = command.SpecialtyZones;

            var updated = await _agentProfileRepository.UpdateAsync(profile.AgentProfileId, profile);
            return _mapper.Map<AgentProfileDto>(updated);
        }
    }
}
