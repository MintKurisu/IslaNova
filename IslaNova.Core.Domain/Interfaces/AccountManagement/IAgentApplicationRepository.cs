using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Core.Domain.Interfaces.Base;

namespace IslaNova.Core.Domain.Interfaces.AccountManagement
{
    public interface IAgentApplicationRepository : IGenericRepository<AgentApplication>
    {
        Task<AgentApplication?> GetByUserIdAsync(string userId);
        Task<bool> ExistsByUserIdAsync(string userId);
    }
}
