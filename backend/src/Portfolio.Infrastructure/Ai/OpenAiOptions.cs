namespace Portfolio.Infrastructure.Ai;

/// <summary>
/// OpenAI provider settings. The API key is NEVER stored in the repository —
/// it comes from user-secrets (Development) or the
/// <c>Ai__OpenAi__ApiKey</c> environment variable (production).
/// </summary>
public sealed class OpenAiOptions
{
    public const string SectionName = "Ai:OpenAi";

    /// <summary>Empty = assistant disabled (endpoints return 503, UI explains).</summary>
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    public string ChatModel { get; set; } = "gpt-4o-mini";

    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}