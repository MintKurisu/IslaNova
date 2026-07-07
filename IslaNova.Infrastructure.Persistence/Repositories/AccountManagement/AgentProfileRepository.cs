using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Infrastructure.Persistence.Repositories.AccountManagement
{
    public class AgentProfileRepository : GenericRepository<AgentProfile>, IAgentProfileRepository
    {
        public AgentProfileRepository(IslaNovaContext context) : base(context)
        {
        }

        public async Task<AgentProfile?> GetByAgentIdAsync(string agentId)
        {
            return await _context.AgentProfiles
                .FirstOrDefaultAsync(ap => ap.AgentId == agentId);
        }
    }
}
