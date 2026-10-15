namespace Portfolio.Application.Services;

/// <summary>
/// Rebuilds the RAG index (<c>content_chunks</c>) from the portfolio's source
/// content. Idempotent: every run replaces the previous index atomically.
/// </summary>
public interface IPortfolioIndexer
{
    /// <summary>
    /// Deletes and rebuilds all chunks, embedding them in batches.
    /// Returns the number of chunks written (0 when the embedding service is
    /// not configured — indexing is skipped, not an error).
    /// </summary>
    Task<int> ReindexAsync(CancellationToken cancellationToken = default);
}