namespace Portfolio.Application.Services;

/// <summary>
/// Turns text into embedding vectors via the configured provider (OpenAI).
/// <see cref="IsConfigured"/> is false when no API key is present — callers
/// must degrade gracefully instead of calling the provider.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>True when an API key is configured and the service can be used.</summary>
    bool IsConfigured { get; }

    /// <summary>Embeds a single text.</summary>
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>Embeds a batch of texts in one provider call.</summary>
    Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);
}