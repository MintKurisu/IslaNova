namespace IslaNova.Core.Application.Dtos.ChatMessage
{
    public class ChatMessageDto
    {
        public int ChatMessageId { get; set; }
        public int PropertyId { get; set; }
        public required string ClientId { get; set; }
        public string? ClientName { get; set; }
        public required string AgentId { get; set; }
        public string? AgentName { get; set; }
        public required string SenderId { get; set; }
        public required string Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}
