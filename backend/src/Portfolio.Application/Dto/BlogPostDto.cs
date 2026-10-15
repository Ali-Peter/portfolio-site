namespace Portfolio.Application.Dto;

/// <summary>Blog list/detail payload returned by the public API.</summary>
public sealed record BlogPostDto(
    string Slug,
    string Title,
    string Excerpt,
    string BodyMarkdown,
    string? CoverImageUrl,
    DateTime? PublishedAt,
    int ReadingMinutes,
    int ViewCount,
    IReadOnlyList<string> Tags);
