namespace IslaNova.Core.Application.Interfaces.AI
{
    /// <summary>
    /// Abstracts the AI chat completion. In production backed by OpenAI gpt-4o-mini.
    /// </summary>
    public interface IChatCompletionService
    {
        /// <summary>
        /// Given a system prompt + user question, returns an AI-generated response.
        /// </summary>
        Task<string> CompleteAsync(string systemPrompt, string userQuestion, CancellationToken ct = default);
    }
}
