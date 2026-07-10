using IslaNova.Core.Application.Dtos.Chatbot;
using IslaNova.Core.Application.Interfaces.AI;
using IslaNova.Core.Domain.Interfaces.AI;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;

namespace IslaNova.Core.Application.Features.Chatbot.Queries.AskChatbot
{
    public class AskChatbotQuery : IRequest<ChatbotResponseDto>
    {
        /// <summary>Natural language question from an anonymous visitor.</summary>
        public required string Question { get; set; }
    }

    public class AskChatbotQueryHandler : IRequestHandler<AskChatbotQuery, ChatbotResponseDto>
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IPropertyEmbeddingRepository _embeddingRepository;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ILogger<AskChatbotQueryHandler> _logger;

        // The number of properties to retrieve from the vector store as context.
        private const int TopK = 5;
        // Minimum cosine similarity to consider a result relevant.
        private const double SimilarityThreshold = 0.65;

        public AskChatbotQueryHandler(
            IEmbeddingService embeddingService,
            IPropertyEmbeddingRepository embeddingRepository,
            IChatCompletionService chatCompletionService,
            ILogger<AskChatbotQueryHandler> logger)
        {
            _embeddingService = embeddingService;
            _embeddingRepository = embeddingRepository;
            _chatCompletionService = chatCompletionService;
            _logger = logger;
        }

        public async Task<ChatbotResponseDto> Handle(AskChatbotQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Chatbot query received: {Question}", query.Question);

            // Step 1 — Generate embedding for the user's question
            float[] questionEmbedding;
            try
            {
                questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(query.Question, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate embedding for question.");
                return new ChatbotResponseDto
                {
                    Answer = "Lo siento, no pude procesar tu consulta en este momento. Por favor intenta nuevamente.",
                    RelatedProperties = []
                };
            }

            // Step 2 — Search for similar properties in the vector store
            var similarProperties = await _embeddingRepository.SearchSimilarAsync(
                questionEmbedding, TopK, SimilarityThreshold, cancellationToken);

            _logger.LogInformation("Found {Count} similar properties for query.", similarProperties.Count);

            // Step 3 — Build the system prompt with retrieved context (RAG)
            string systemPrompt = BuildSystemPrompt(similarProperties);

            // Step 4 — Get AI-generated answer
            string answer;
            try
            {
                answer = await _chatCompletionService.CompleteAsync(systemPrompt, query.Question, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get chat completion.");
                answer = "Lo siento, el asistente no está disponible en este momento. Puedes comunicarte directamente con nuestros agentes.";
            }

            // Step 5 — Build response with related properties as references
            return new ChatbotResponseDto
            {
                Answer = answer,
                RelatedProperties = similarProperties.Select(p => new PropertyReferenceDto
                {
                    PropertyId = p.PropertyId,
                    PlainText = p.PlainText,
                    Similarity = p.Similarity
                }).ToList()
            };
        }

        private static string BuildSystemPrompt(List<(int PropertyId, string PlainText, double Similarity)> properties)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Eres un asistente inmobiliario de IslaNova, una plataforma de bienes raíces.");
            sb.AppendLine("Tu función es ayudar a visitantes a encontrar propiedades que se ajusten a sus necesidades.");
            sb.AppendLine("Responde siempre en español, de forma amable, concisa y útil.");
            sb.AppendLine("Basa tus respuestas ÚNICAMENTE en la información de propiedades proporcionada a continuación.");
            sb.AppendLine("Si no hay propiedades relevantes o no puedes responder con la información disponible, indícalo claramente.");
            sb.AppendLine("No inventes datos, precios o características que no estén en el contexto.");
            sb.AppendLine();

            if (properties.Count > 0)
            {
                sb.AppendLine("=== PROPIEDADES DISPONIBLES (contexto) ===");
                for (int i = 0; i < properties.Count; i++)
                {
                    sb.AppendLine($"[Propiedad {i + 1} | ID: {properties[i].PropertyId}]");
                    sb.AppendLine(properties[i].PlainText);
                    sb.AppendLine();
                }
                sb.AppendLine("==========================================");
            }
            else
            {
                sb.AppendLine("No se encontraron propiedades que coincidan con la consulta del usuario.");
                sb.AppendLine("Informa al usuario que puede contactar a un agente para más ayuda.");
            }

            return sb.ToString();
        }
    }
}
