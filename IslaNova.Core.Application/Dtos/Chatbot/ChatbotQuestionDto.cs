using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Dtos.Chatbot
{
    public class ChatbotQuestionDto
    {
        [SwaggerParameter(Description = "The user's question or query about properties")]
        public required string Question { get; set; }
    }
}
