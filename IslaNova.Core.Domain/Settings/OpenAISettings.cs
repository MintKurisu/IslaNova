namespace IslaNova.Core.Domain.Settings
{
    public class OpenAISettings
    {
        /// <summary>Your OpenAI API Key (stored in appsettings / environment variables).</summary>
        public required string ApiKey { get; set; }

        /// <summary>Embedding model. Default: text-embedding-3-small (1536 dims).</summary>
        public string EmbeddingModel { get; set; } = "text-embedding-3-small";

        /// <summary>Number of dimensions for the embedding vector.</summary>
        public int EmbeddingDimensions { get; set; } = 1536;

        /// <summary>Chat completion model for the chatbot. Default: gpt-4o-mini.</summary>
        public string ChatModel { get; set; } = "gpt-4o-mini";

        /// <summary>Max tokens for chat completion response.</summary>
        public int MaxTokens { get; set; } = 1024;
    }
}
