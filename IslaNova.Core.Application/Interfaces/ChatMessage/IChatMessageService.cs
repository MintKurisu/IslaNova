using IslaNova.Core.Application.Dtos.ChatMessage;
using IslaNova.Core.Application.Interfaces.Base;

namespace IslaNova.Core.Application.Interfaces.ChatMessage
{
    public interface IChatMessageService : IGenericService<ChatMessageDto>
    {
        Task<List<ChatMessageDto>> GetChatMessagesByPropertyAndClientAsync(int propertyId, string clientId);
        Task<List<string>> GetClientsWithChatByPropertyIdAsync(int propertyId);
        Task<ChatMessageDto?> SendMessageAsync(CreateChatMessageDto dto);
    }
}
