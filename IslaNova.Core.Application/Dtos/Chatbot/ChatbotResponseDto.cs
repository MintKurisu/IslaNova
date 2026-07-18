namespace IslaNova.Core.Application.Dtos.Chatbot
{
    public class ChatbotResponseDto
    {
        /// <summary>The AI-generated natural language answer.</summary>
        public required string Answer { get; set; }

        /// <summary>
        /// List of properties that were retrieved from the vector store
        /// and used as context to generate the answer.
        /// </summary>
        public List<PropertyReferenceDto> RelatedProperties { get; set; } = [];

        /// <summary>Whether any relevant properties were found.</summary>
        public bool HasResults => RelatedProperties.Any();
    }

    public class PropertyReferenceDto
    {
        public int PropertyId { get; set; }
        public string PlainText { get; set; } = string.Empty;

        /// <summary>Cosine similarity score (0.0 - 1.0).</summary>
        public double Similarity { get; set; }
    }
}
