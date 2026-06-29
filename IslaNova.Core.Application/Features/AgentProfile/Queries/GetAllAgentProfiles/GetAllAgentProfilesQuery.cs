using AutoMapper;
using IslaNova.Core.Application.Dtos.AgentProfile;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.AgentProfile.Queries.GetAllAgentProfiles
{
    public class GetAllAgentProfilesQuery : IRequest<IList<AgentProfileDto>> { }

    public class GetAllAgentProfilesQueryHandler : IRequestHandler<GetAllAgentProfilesQuery, IList<AgentProfileDto>>
    {
        private readonly IAgentProfileRepository _agentProfileRepository;
        private readonly IMapper _mapper;

        public GetAllAgentProfilesQueryHandler(
            IAgentProfileRepository agentProfileRepository,
            IMapper mapper)
        {
            _agentProfileRepository = agentProfileRepository;
            _mapper = mapper;
        }

        public async Task<IList<AgentProfileDto>> Handle(GetAllAgentProfilesQuery query, CancellationToken cancellationToken)
        {
            var profiles = await _agentProfileRepository
                .GetAllQuery()
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<AgentProfileDto>>(profiles);
        }
    }
}
