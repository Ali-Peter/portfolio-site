namespace Portfolio.Application.Services;

/// <summary>
/// Streams a chat completion token-by-token from the configured provider (OpenAI).
/// Implementations throw <see cref="Exceptions.AiUnavailableException"/> when
/// the provider is not configured.
/// </summary>
public interface IChatCompletionService
{
    /// <summary>Yields answer text increments for the given prompt pair.</summary>
    IAsyncEnumerable<string> StreamCompletionAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}