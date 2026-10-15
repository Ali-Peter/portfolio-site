namespace Portfolio.Application.Dto;

/// <summary>A portfolio content slice that grounded an assistant answer.</summary>
public sealed record ChatSourceDto(
    string Type,
    string Title,
    string? Slug);