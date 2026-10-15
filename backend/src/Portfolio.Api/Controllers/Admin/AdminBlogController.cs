using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>Admin blog CRUD (includes drafts/unpublished; soft-delete on remove).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/blog")]
[Produces("application/json")]
public sealed class AdminBlogController : AdminControllerBase
{
    private readonly IAdminContentService _content;
    private readonly IValidator<UpsertBlogPostRequest> _validator;

    public AdminBlogController(
        IAdminContentService content,
        IValidator<UpsertBlogPostRequest> validator,
        IServiceProvider services,
        ILogger<AdminBlogController> logger)
        : base(services, logger)
    {
        _content = content;
        _validator = validator;
    }

    /// <summary>All posts for the admin list (newest first, incl. drafts).</summary>
    /// <response code="200">Returns the admin blog rows.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    [HttpGet(Name = "AdminGetBlogPosts")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminBlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AdminBlogPostDto>>> GetPosts(
        CancellationToken cancellationToken = default)
    {
        var items = await _content.GetBlogPostsAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>Single post for the admin editor.</summary>
    /// <response code="200">Returns the post.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No post uses that id.</response>
    [HttpGet("{id:guid}", Name = "AdminGetBlogPost")]
    [ProducesResponseType(typeof(AdminBlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminBlogPostDto>> GetPost(
        Guid id, CancellationToken cancellationToken = default)
    {
        var post = await _content.GetBlogPostAsync(id, cancellationToken);
        return Ok(post);
    }

    /// <summary>Create a post (published ones appear on the public site immediately).</summary>
    /// <response code="201">Returns the created post.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="422">Request validation failed (incl. duplicate slug).</response>
    [HttpPost(Name = "AdminCreateBlogPost")]
    [ProducesResponseType(typeof(AdminBlogPostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AdminBlogPostDto>> CreatePost(
        [FromBody] UpsertBlogPostRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var created = await _content.CreateBlogPostAsync(request, Actor, cancellationToken);
        QueueReindex();
        return CreatedAtRoute(nameof(GetPost), new { id = created.Id, version = "1" }, created);
    }

    /// <summary>Replace a post.</summary>
    /// <response code="200">Returns the updated post.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No post uses that id.</response>
    /// <response code="422">Request validation failed (incl. duplicate slug).</response>
    [HttpPut("{id:guid}", Name = "AdminUpdateBlogPost")]
    [ProducesResponseType(typeof(AdminBlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AdminBlogPostDto>> UpdatePost(
        Guid id, [FromBody] UpsertBlogPostRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var updated = await _content.UpdateBlogPostAsync(id, request, Actor, cancellationToken);
        QueueReindex();
        return Ok(updated);
    }

    /// <summary>Soft-delete a post (hidden from the public site, history kept).</summary>
    /// <response code="204">Post deleted.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No post uses that id.</response>
    [HttpDelete("{id:guid}", Name = "AdminDeleteBlogPost")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken = default)
    {
        await _content.DeleteBlogPostAsync(id, cancellationToken);
        QueueReindex();
        return NoContent();
    }
}
