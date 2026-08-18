using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Features.Chatbot.Queries.AskChatbot;
using IslaNova.Core.Application.Interfaces.AI;
using IslaNova.Core.Domain.Interfaces.AI;

namespace IslaNova.Tests.UnitTests.Features.Chatbot
{
    /// <summary>
    /// Unit tests for AskChatbotQueryHandler.
    /// All external dependencies (OpenAI, pgvector) are mocked — no real HTTP calls are made.
    /// </summary>
    public class AskChatbotQueryHandlerTests
    {
        private readonly Mock<IEmbeddingService> _embeddingServiceMock;
        private readonly Mock<IPropertyEmbeddingRepository> _embeddingRepositoryMock;
        private readonly Mock<IChatCompletionService> _chatCompletionServiceMock;
        private readonly Mock<ILogger<AskChatbotQueryHandler>> _loggerMock;
        private readonly AskChatbotQueryHandler _handler;

        // Deterministic dummy embedding (1536-dimensional unit vector)
        private static readonly float[] DummyEmbedding = Enumerable.Repeat(0.01f, 1536).ToArray();

        public AskChatbotQueryHandlerTests()
        {
            _embeddingServiceMock      = new Mock<IEmbeddingService>();
            _embeddingRepositoryMock   = new Mock<IPropertyEmbeddingRepository>();
            _chatCompletionServiceMock = new Mock<IChatCompletionService>();
            _loggerMock                = new Mock<ILogger<AskChatbotQueryHandler>>();

            _handler = new AskChatbotQueryHandler(
                _embeddingServiceMock.Object,
                _embeddingRepositoryMock.Object,
                _chatCompletionServiceMock.Object,
                _loggerMock.Object);
        }

        // ─── Happy Path ───────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_Should_Return_Answer_When_Properties_Are_Found()
        {
            // Arrange
            const string question = "Busco una casa con piscina cerca de la playa";
            const string expectedAnswer = "Tenemos 2 propiedades que podrían interesarte.";

            var similarProperties = new List<(int PropertyId, string PlainText, double Similarity)>
            {
                (1, "Casa de 3 habitaciones con piscina, vista al mar, Punta Cana.", 0.92),
                (2, "Villa familiar cerca del mar, 4 habitaciones, piscina privada.", 0.85)
            };

            _embeddingServiceMock
                .Setup(s => s.GenerateEmbeddingAsync(question, It.IsAny<CancellationToken>()))
                .ReturnsAsync(DummyEmbedding);

            _embeddingRepositoryMock
                .Setup(r => r.SearchSimilarAsync(DummyEmbedding, It.IsAny<int>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(similarProperties);

            _chatCompletionServiceMock
                .Setup(c => c.CompleteAsync(It.IsAny<string>(), question, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAnswer);

            var query = new AskChatbotQuery { Question = question };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Answer.Should().Be(expectedAnswer);
            result.HasResults.Should().BeTrue();
            result.RelatedProperties.Should().HaveCount(2);
            result.RelatedProperties[0].PropertyId.Should().Be(1);
            result.RelatedProperties[0].Similarity.Should().BeApproximately(0.92, 0.001);
            result.RelatedProperties[1].PropertyId.Should().Be(2);
        }

        [Fact]
        public async Task Handle_Should_Return_Answer_When_No_Properties_Found()
        {
            // Arrange — empty vector store result (valid scenario: no matching properties)
            const string question = "¿Tienen propiedades en Marte?";
            const string expectedAnswer = "No encontré propiedades que coincidan. Te recomiendo contactar a un agente.";

            _embeddingServiceMock
                .Setup(s => s.GenerateEmbeddingAsync(question, It.IsAny<CancellationToken>()))
                .ReturnsAsync(DummyEmbedding);

            _embeddingRepositoryMock
                .Setup(r => r.SearchSimilarAsync(DummyEmbedding, It.IsAny<int>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<(int, string, double)>());

            _chatCompletionServiceMock
                .Setup(c => c.CompleteAsync(It.IsAny<string>(), question, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAnswer);

            var query = new AskChatbotQuery { Question = question };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Answer.Should().Be(expectedAnswer);
            result.HasResults.Should().BeFalse();
            result.RelatedProperties.Should().BeEmpty();
        }

        // ─── Error Handling ───────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_Should_Return_Fallback_When_EmbeddingService_Throws()
        {
            // Arrange — simulate OpenAI embedding failure
            const string question = "¿Cuánto cuesta una villa?";

            _embeddingServiceMock
                .Setup(s => s.GenerateEmbeddingAsync(question, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("OpenAI endpoint unavailable"));

            var query = new AskChatbotQuery { Question = question };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert — must gracefully degrade, never throw to the caller
            result.Should().NotBeNull();
            result.Answer.Should().NotBeNullOrEmpty();
            result.Answer.Should().Contain("Lo siento");
            result.RelatedProperties.Should().BeEmpty();

            // The embedding repository and chat completion service should NOT be called
            _embeddingRepositoryMock.Verify(
                r => r.SearchSimilarAsync(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _chatCompletionServiceMock.Verify(
                c => c.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Return_Fallback_When_ChatCompletion_Throws()
        {
            // Arrange — simulate GPT failure after successful embedding + retrieval
            const string question = "Busco apartamento económico";

            _embeddingServiceMock
                .Setup(s => s.GenerateEmbeddingAsync(question, It.IsAny<CancellationToken>()))
                .ReturnsAsync(DummyEmbedding);

            _embeddingRepositoryMock
                .Setup(r => r.SearchSimilarAsync(DummyEmbedding, It.IsAny<int>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<(int, string, double)> { (5, "Apartamento céntrico, 2 hab.", 0.75) });

            _chatCompletionServiceMock
                .Setup(c => c.CompleteAsync(It.IsAny<string>(), question, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("GPT rate limit exceeded"));

            var query = new AskChatbotQuery { Question = question };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert — fallback message should mention agents
            result.Should().NotBeNull();
            result.Answer.Should().Contain("agentes");
            // RelatedProperties still populated from the vector search that DID succeed
            result.RelatedProperties.Should().HaveCount(1);
        }

        // ─── RAG Context Construction ─────────────────────────────────────────────

        [Fact]
        public async Task Handle_Should_Call_ChatCompletion_With_System_Prompt_Containing_Properties()
        {
            // Arrange — verify that the system prompt passed to GPT contains property info (RAG)
            const string question = "Quiero saber sobre departamentos disponibles";
            string? capturedSystemPrompt = null;

            _embeddingServiceMock
                .Setup(s => s.GenerateEmbeddingAsync(question, It.IsAny<CancellationToken>()))
                .ReturnsAsync(DummyEmbedding);

            _embeddingRepositoryMock
                .Setup(r => r.SearchSimilarAsync(DummyEmbedding, It.IsAny<int>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<(int, string, double)>
                {
                    (10, "Departamento moderno, 2 habitaciones, zona universitaria.", 0.88)
                });

            _chatCompletionServiceMock
                .Setup(c => c.CompleteAsync(It.IsAny<string>(), question, It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((sysPrompt, _, _) => capturedSystemPrompt = sysPrompt)
                .ReturnsAsync("Tenemos un departamento moderno disponible.");

            var query = new AskChatbotQuery { Question = question };

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert — system prompt must include the property text (RAG injection)
            capturedSystemPrompt.Should().NotBeNullOrEmpty();
            capturedSystemPrompt.Should().Contain("IslaNova");
            capturedSystemPrompt.Should().Contain("ID: 10");
            capturedSystemPrompt.Should().Contain("Departamento moderno");
        }

        [Fact]
        public async Task Handle_Should_Call_ChatCompletion_With_No_Properties_Fallback_Prompt()
        {
            // Arrange — when no properties are retrieved, system prompt must indicate it
            const string question = "¿Tienen condominios en la luna?";
            string? capturedSystemPrompt = null;

            _embeddingServiceMock
                .Setup(s => s.GenerateEmbeddingAsync(question, It.IsAny<CancellationToken>()))
                .ReturnsAsync(DummyEmbedding);

            _embeddingRepositoryMock
                .Setup(r => r.SearchSimilarAsync(DummyEmbedding, It.IsAny<int>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<(int, string, double)>());

            _chatCompletionServiceMock
                .Setup(c => c.CompleteAsync(It.IsAny<string>(), question, It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((sysPrompt, _, _) => capturedSystemPrompt = sysPrompt)
                .ReturnsAsync("Lo siento, no tenemos propiedades que coincidan.");

            var query = new AskChatbotQuery { Question = question };

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert — system prompt must inform GPT that there are no matching properties
            capturedSystemPrompt.Should().Contain("No se encontraron propiedades");
        }

        // ─── Orchestration / Interaction Order ────────────────────────────────────

        [Fact]
        public async Task Handle_Should_Call_Services_In_Correct_Order()
        {
            // Arrange
            var callOrder = new List<string>();

            _embeddingServiceMock
                .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback(() => callOrder.Add("embedding"))
                .ReturnsAsync(DummyEmbedding);

            _embeddingRepositoryMock
                .Setup(r => r.SearchSimilarAsync(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
                .Callback(() => callOrder.Add("search"))
                .ReturnsAsync(new List<(int, string, double)>());

            _chatCompletionServiceMock
                .Setup(c => c.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback(() => callOrder.Add("completion"))
                .ReturnsAsync("Respuesta del asistente.");

            var query = new AskChatbotQuery { Question = "Test question" };

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert — RAG pipeline must follow the exact order: embed → search → complete
            callOrder.Should().Equal("embedding", "search", "completion");
        }
    }
}
