using IslaNova.Core.Application.Interfaces.AI;
using IslaNova.Core.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace IslaNova.Infrastructure.AI.Services
{
    /// <summary>
    /// Chat completion service using OpenAI gpt-4o-mini model.
    /// Receives a system prompt with property context and the user's question,
    /// and returns a natural language answer in Spanish.
    /// </summary>
    public class OpenAIChatCompletionService : IChatCompletionService
    {
        private readonly ChatClient _client;
        private readonly OpenAISettings _settings;
        private readonly ILogger<OpenAIChatCompletionService> _logger;

        public OpenAIChatCompletionService(IOptions<OpenAISettings> settings, ILogger<OpenAIChatCompletionService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            _client = new ChatClient(_settings.ChatModel, _settings.ApiKey);
        }

        public async Task<string> CompleteAsync(string systemPrompt, string userQuestion, CancellationToken ct = default)
        {
            _logger.LogDebug("Sending chat completion request. Model: {Model}", _settings.ChatModel);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userQuestion)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = _settings.MaxTokens
            };

            var response = await _client.CompleteChatAsync(messages, options, cancellationToken: ct);
            var completion = response.Value;

            var result = completion.Content.FirstOrDefault()?.Text ?? string.Empty;

            _logger.LogDebug("Chat completion received. Tokens used: {Tokens}", completion.Usage?.TotalTokenCount);

            return result;
        }
    }
}
