using Portfolio.Application.Dto;

namespace Portfolio.Application.Services;

/// <summary>Public blog queries (published posts only).</summary>
public interface IBlogService
{
    Task<PagedResult<BlogPostDto>> GetPostsAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task<BlogPostDto> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}
