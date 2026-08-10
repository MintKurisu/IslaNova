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
        private const int TopK = 10;
        // Minimum cosine similarity to consider a result relevant.
        private const double SimilarityThreshold = 0.35;

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

            sb.AppendLine("Eres el asistente inmobiliario de IslaNova, una plataforma de bienes raíces.");
            sb.AppendLine();

            sb.AppendLine("OBJETIVO:");
            sb.AppendLine("Tu objetivo es ayudar al usuario a encontrar propiedades disponibles que se ajusten a sus necesidades, utilizando únicamente la información proporcionada en el contexto.");
            sb.AppendLine();

            sb.AppendLine("REGLAS:");
            sb.AppendLine("1. Responde siempre en español, de forma amable, clara, natural y concisa.");
            sb.AppendLine("2. Utiliza las propiedades del contexto como fuente principal para responder preguntas relacionadas con propiedades.");
            sb.AppendLine("3. Nunca inventes propiedades, precios, ubicaciones, habitaciones, baños, amenidades, agentes u otras características.");
            sb.AppendLine("4. Utiliza únicamente información que aparezca explícitamente en el contexto.");
            sb.AppendLine("5. Si una propiedad coincide parcialmente con la consulta, puedes mostrarla indicando únicamente las características que realmente coinciden.");
            sb.AppendLine("6. Si existen varias propiedades relevantes, presenta primero las más relevantes.");
            sb.AppendLine("7. Si el usuario solicita propiedades, proporciona sus características más importantes, como precio, ubicación, tipo, habitaciones y baños, cuando estén disponibles.");
            sb.AppendLine("8. Si el usuario pregunta por una propiedad específica utilizando su ID, código, ubicación o características, utiliza la información correspondiente del contexto.");
            sb.AppendLine("9. Si la información necesaria para responder no está disponible en el contexto, indícalo claramente.");
            sb.AppendLine("10. No inventes información para completar datos faltantes.");
            sb.AppendLine("11. Los precios, cantidades y características deben mantenerse exactamente como aparecen en el contexto.");
            sb.AppendLine("12. Si no existe una coincidencia exacta pero existen alternativas razonablemente similares, muestra esas alternativas.");
            sb.AppendLine("13. Si no hay resultados relevantes, no afirmes que no existen propiedades en IslaNova. Indica únicamente que no encontraste coincidencias relevantes para esa consulta.");
            sb.AppendLine("14. Cuando no encuentres resultados adecuados, sugiere al usuario modificar algún criterio de búsqueda, como ubicación, precio, tipo de propiedad o cantidad de habitaciones.");
            sb.AppendLine();

            if (properties.Count > 0)
            {
                sb.AppendLine("=== PROPIEDADES RECUPERADAS ===");
                sb.AppendLine("Las siguientes propiedades fueron recuperadas mediante una búsqueda semántica.");
                sb.AppendLine("Utilízalas como contexto para responder al usuario.");
                sb.AppendLine();

                for (int i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];

                    sb.AppendLine(
                        $"[Propiedad {i + 1} | ID: {property.PropertyId} | " +
                        $"Relevancia: {property.Similarity:F3}]");

                    sb.AppendLine(property.PlainText);
                    sb.AppendLine();
                }

                sb.AppendLine("=================================");
            }
            else
            {
                sb.AppendLine("=== SIN RESULTADOS ===");
                sb.AppendLine("La búsqueda semántica no encontró propiedades con suficiente relevancia para esta consulta.");
                sb.AppendLine("No asumas que no existen propiedades que cumplan los criterios del usuario.");
                sb.AppendLine("Indica que no encontraste coincidencias relevantes y sugiere modificar los criterios de búsqueda.");
                sb.AppendLine("=================================");
            }

            return sb.ToString();
        }
    }
}
