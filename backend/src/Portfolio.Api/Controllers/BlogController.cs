using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers;

/// <summary>Public blog (published posts only).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/blog")]
[Produces("application/json")]
public sealed class BlogController : ControllerBase
{
    private readonly IBlogService _blog;

    public BlogController(IBlogService blog)
    {
        _blog = blog;
    }

    /// <summary>Paged published posts, newest first, with optional title/excerpt search.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page (1–100).</param>
    /// <param name="search">Optional case-insensitive substring over title and excerpt.</param>
    /// <param name="cancellationToken">Propagates request cancellation.</param>
    /// <response code="200">Returns the page.</response>
    /// <response code="422">Paging values are out of range.</response>
    [HttpGet(Name = nameof(GetPosts))]
    [ProducesResponseType(typeof(PagedResult<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PagedResult<BlogPostDto>>> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _blog.GetPostsAsync(page, pageSize, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>Single published post by its URL slug.</summary>
    /// <response code="200">Returns the post.</response>
    /// <response code="404">No published post uses that slug.</response>
    /// <response code="422">The slug is empty.</response>
    [HttpGet("{slug}", Name = nameof(GetPostBySlug))]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BlogPostDto>> GetPostBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var post = await _blog.GetPostBySlugAsync(slug, cancellationToken);
        return Ok(post);
    }
}
