using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>
/// A portfolio project (case study) shown on the public site.
/// </summary>
public class Project : BaseEntity
{
    /// <summary>URL-safe unique identifier, e.g. "outbound-ai-backend".</summary>
    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>Short one/two sentence card summary.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>Full case-study description (Markdown allowed).</summary>
    public string Description { get; set; } = string.Empty;

    public string? RepoUrl { get; set; }

    public string? DemoUrl { get; set; }

    public string? CoverImageUrl { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Completed;

    /// <summary>Whether the project is highlighted on the home page.</summary>
    public bool Featured { get; set; }

    public DateOnly? StartedOn { get; set; }

    public DateOnly? CompletedOn { get; set; }

    /// <summary>Lower numbers sort first on the public projects page.</summary>
    public int SortOrder { get; set; }

    /// <summary>Technology labels, stored as a PostgreSQL <c>text[]</c>.</summary>
    public string[] Technologies { get; set; } = [];
}
