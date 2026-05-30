using IslaNova.Core.Domain.Entities.UserInteraction;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Base;

namespace IslaNova.Infrastructure.Persistence.Repositories.UserInteraction
{
    public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(IslaNovaContext context) : base(context)
        {

        }
    }
}
