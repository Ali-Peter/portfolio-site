using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>Admin project CRUD (includes drafts; soft-delete on remove).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/projects")]
[Produces("application/json")]
public sealed class AdminProjectsController : AdminControllerBase
{
    private readonly IAdminContentService _content;
    private readonly IValidator<UpsertProjectRequest> _validator;

    public AdminProjectsController(
        IAdminContentService content,
        IValidator<UpsertProjectRequest> validator,
        IServiceProvider services,
        ILogger<AdminProjectsController> logger)
        : base(services, logger)
    {
        _content = content;
        _validator = validator;
    }

    /// <summary>All projects for the admin list (sort-order).</summary>
    /// <response code="200">Returns the admin project rows.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    [HttpGet(Name = "AdminGetProjects")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AdminProjectDto>>> GetProjects(
        CancellationToken cancellationToken = default)
    {
        var items = await _content.GetProjectsAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>Single project for the admin editor.</summary>
    /// <response code="200">Returns the project.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No project uses that id.</response>
    [HttpGet("{id:guid}", Name = "AdminGetProject")]
    [ProducesResponseType(typeof(AdminProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProjectDto>> GetProject(
        Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _content.GetProjectAsync(id, cancellationToken);
        return Ok(project);
    }

    /// <summary>Create a project (visible on the public site immediately).</summary>
    /// <response code="201">Returns the created project.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="422">Request validation failed (incl. duplicate slug).</response>
    [HttpPost(Name = "AdminCreateProject")]
    [ProducesResponseType(typeof(AdminProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AdminProjectDto>> CreateProject(
        [FromBody] UpsertProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var created = await _content.CreateProjectAsync(request, Actor, cancellationToken);
        QueueReindex();
        return CreatedAtRoute(nameof(GetProject), new { id = created.Id, version = "1" }, created);
    }

    /// <summary>Replace a project.</summary>
    /// <response code="200">Returns the updated project.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No project uses that id.</response>
    /// <response code="422">Request validation failed (incl. duplicate slug).</response>
    [HttpPut("{id:guid}", Name = "AdminUpdateProject")]
    [ProducesResponseType(typeof(AdminProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AdminProjectDto>> UpdateProject(
        Guid id, [FromBody] UpsertProjectRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var updated = await _content.UpdateProjectAsync(id, request, Actor, cancellationToken);
        QueueReindex();
        return Ok(updated);
    }

    /// <summary>Soft-delete a project (hidden from the public site, history kept).</summary>
    /// <response code="204">Project deleted.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No project uses that id.</response>
    [HttpDelete("{id:guid}", Name = "AdminDeleteProject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken cancellationToken = default)
    {
        await _content.DeleteProjectAsync(id, cancellationToken);
        QueueReindex();
        return NoContent();
    }
}
