using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Core.Domain.Interfaces.Base;

namespace IslaNova.Core.Domain.Interfaces.AccountManagement
{
    public interface IAgentProfileRepository : IGenericRepository<AgentProfile>
    {
        Task<AgentProfile?> GetByAgentIdAsync(string agentId);
    }
}
