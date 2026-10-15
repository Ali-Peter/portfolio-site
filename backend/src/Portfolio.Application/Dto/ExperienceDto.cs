namespace Portfolio.Application.Dto;

/// <summary>Experience/resume entry payload returned by the public API.</summary>
public sealed record ExperienceDto(
    string Company,
    string Title,
    string Description,
    IReadOnlyList<string> Technologies,
    DateOnly? StartedOn,
    DateOnly? EndedOn,
    bool IsCurrent,
    int SortOrder);
