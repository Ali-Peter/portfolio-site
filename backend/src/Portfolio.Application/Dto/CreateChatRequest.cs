namespace Portfolio.Application.Dto;

/// <summary>A question submitted to the portfolio AI assistant.</summary>
public sealed record CreateChatRequest(
    string Message);