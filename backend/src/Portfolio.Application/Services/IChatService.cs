using Portfolio.Application.Dto;

namespace Portfolio.Application.Services;

/// <summary>
/// The public assistant surface: validates a question, retrieves grounding
/// context from the portfolio index, and streams a grounded answer.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Streams the answer as <see cref="ChatStreamEvent"/> frames
    /// (<c>sources</c> → <c>delta</c>… → <c>done</c>).
    /// </summary>
    IAsyncEnumerable<ChatStreamEvent> AskAsync(
        CreateChatRequest request,
        CancellationToken cancellationToken = default);
}