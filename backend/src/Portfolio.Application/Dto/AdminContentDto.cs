using Portfolio.Domain.Enums;

namespace Portfolio.Application.Dto;

// ---------------------------------------------------------------------------
// Projects (admin)
// ---------------------------------------------------------------------------

/// <summary>Project payload for admin create/update (Id/Slug resolved by the service).</summary>
public sealed record UpsertProjectRequest(
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
    int SortOrder,
    IReadOnlyList<string>? Technologies);

// ---------------------------------------------------------------------------
// Blog posts (admin)
// ---------------------------------------------------------------------------

/// <summary>Blog post payload for admin create/update. PublishedAt null = draft.</summary>
public sealed record UpsertBlogPostRequest(
    string Slug,
    string Title,
    string Excerpt,
    string BodyMarkdown,
    string? CoverImageUrl,
    DateTime? PublishedAt,
    int ReadingMinutes,
    bool IsPublished,
    IReadOnlyList<string>? Tags);

// ---------------------------------------------------------------------------
// Skills (admin)
// ---------------------------------------------------------------------------

/// <summary>Skill payload for admin create/update.</summary>
public sealed record UpsertSkillRequest(
    string Name,
    SkillCategory Category,
    SkillProficiency Proficiency,
    int SortOrder);

// ---------------------------------------------------------------------------
// Experience (admin)
// ---------------------------------------------------------------------------

/// <summary>Experience payload for admin create/update.</summary>
public sealed record UpsertExperienceRequest(
    string Company,
    string Title,
    string Description,
    IReadOnlyList<string>? Technologies,
    DateOnly? StartedOn,
    DateOnly? EndedOn,
    bool IsCurrent,
    int SortOrder);

// ---------------------------------------------------------------------------
// Admin view models (include soft-deleted-filtered Id + audit info the
// public DTOs intentionally omit)
// ---------------------------------------------------------------------------

/// <summary>Admin project row (includes Id for edit/delete links).</summary>
public sealed record AdminProjectDto(
    Guid Id,
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
    int SortOrder,
    IReadOnlyList<string> Technologies);

/// <summary>Admin blog row (includes drafts + Id).</summary>
public sealed record AdminBlogPostDto(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    string BodyMarkdown,
    string? CoverImageUrl,
    DateTime? PublishedAt,
    int ReadingMinutes,
    int ViewCount,
    bool IsPublished,
    IReadOnlyList<string> Tags);

/// <summary>Admin skill row (includes Id).</summary>
public sealed record AdminSkillDto(
    Guid Id,
    string Name,
    SkillCategory Category,
    SkillProficiency Proficiency,
    int SortOrder);

/// <summary>Admin experience row (includes Id).</summary>
public sealed record AdminExperienceDto(
    Guid Id,
    string Company,
    string Title,
    string Description,
    IReadOnlyList<string> Technologies,
    DateOnly? StartedOn,
    DateOnly? EndedOn,
    bool IsCurrent,
    int SortOrder);

/// <summary>Admin contact inbox row (includes handling state + audit).</summary>
public sealed record AdminContactMessageDto(
    Guid Id,
    string Name,
    string Email,
    string Message,
    bool IsHandled,
    DateTime CreatedAtUtc);
