using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Infrastructure.Persistence.Repositories.AccountManagement
{
    public class AgentApplicationRepository : GenericRepository<AgentApplication>, IAgentApplicationRepository
    {
        public AgentApplicationRepository(IslaNovaContext context) : base(context)
        {
           
        }

        public async Task<AgentApplication?> GetByUserIdAsync(string userId)
        {
            return await _context.AgentApplications
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<bool> ExistsByUserIdAsync(string userId)
        {
            return await _context.AgentApplications
                .AnyAsync(a => a.UserId == userId);
        }
    }
}
