namespace IslaNova.Core.Application.Dtos.ChatMessage
{
    public class CreateChatMessageDto
    {
        public int PropertyId { get; set; }
        public required string ClientId { get; set; }
        public required string AgentId { get; set; }
        public required string SenderId { get; set; }
        public required string Message { get; set; }
    }
}
