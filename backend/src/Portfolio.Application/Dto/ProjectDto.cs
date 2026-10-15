using Portfolio.Domain.Enums;

namespace Portfolio.Application.Dto;

/// <summary>Project card/detail payload returned by the public API.</summary>
public sealed record ProjectDto(
    string Slug,
    string Title,
    string Summary,
    string Description,
    string? RepoUrl,
    string? DemoUrl,
    string? CoverImageUrl,
    ProjectStatus Status,
    bool Featured,
    DateOnly? StartedOn,
    DateOnly? CompletedOn,
    IReadOnlyList<string> Technologies);
