namespace Portfolio.Application.Dto;

/// <summary>
/// One frame of the assistant's Server-Sent Events stream:
/// <c>sources</c> (retrieved context, sent first), <c>delta</c> (answer text
/// increment), or <c>done</c> (terminal frame).
/// </summary>
public sealed record ChatStreamEvent(
    string Type,
    string? Text = null,
    IReadOnlyList<ChatSourceDto>? Sources = null)
{
    public static ChatStreamEvent WithSources(IReadOnlyList<ChatSourceDto> sources) =>
        new("sources", null, sources);

    public static ChatStreamEvent Delta(string text) => new("delta", text);

    public static ChatStreamEvent Done() => new("done");
}