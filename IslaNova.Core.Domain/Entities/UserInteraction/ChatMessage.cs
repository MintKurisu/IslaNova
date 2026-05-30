using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Core.Domain.Entities.UserInteraction
{
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }
        public int PropertyId { get; set; }
        public required string ClientId { get; set; }
        public required string AgentId { get; set; }
        public required string SenderId { get; set; }
        public required string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public Property? Property { get; set; }
    }
}
