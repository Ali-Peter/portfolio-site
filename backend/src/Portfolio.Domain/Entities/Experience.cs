namespace Portfolio.Domain.Entities;

/// <summary>
/// A work/education experience entry for the resume section.
/// Dates are intentionally nullable — seed content only includes facts
/// that exist in the original portfolio.
/// </summary>
public class Experience : BaseEntity
{
    public string Company { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Technology labels, stored as a PostgreSQL <c>text[]</c>.</summary>
    public string[] Technologies { get; set; } = [];

    public DateOnly? StartedOn { get; set; }

    public DateOnly? EndedOn { get; set; }

    /// <summary>True when this is the current role.</summary>
    public bool IsCurrent { get; set; }

    /// <summary>Lower numbers sort first on the resume page.</summary>
    public int SortOrder { get; set; }
}
