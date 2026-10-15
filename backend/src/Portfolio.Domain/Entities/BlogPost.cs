namespace Portfolio.Domain.Entities;

/// <summary>
/// A blog article authored in Markdown and rendered by the frontend.
/// </summary>
public class BlogPost : BaseEntity
{
    /// <summary>URL-safe unique identifier, e.g. "hello-world".</summary>
    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>Card/meta description shown in lists and search results.</summary>
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>Full article body (Markdown).</summary>
    public string BodyMarkdown { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public DateTime? PublishedAt { get; set; }

    /// <summary>Estimated reading time in minutes, precomputed for the card UI.</summary>
    public int ReadingMinutes { get; set; }

    public int ViewCount { get; set; }

    public bool IsPublished { get; set; }

    /// <summary>Tag labels, stored as a PostgreSQL <c>text[]</c>.</summary>
    public string[] Tags { get; set; } = [];
}
