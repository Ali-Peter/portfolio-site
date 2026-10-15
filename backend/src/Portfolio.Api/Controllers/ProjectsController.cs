using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers;

/// <summary>Public project catalogue.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects")]
[Produces("application/json")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects)
    {
        _projects = projects;
    }

    /// <summary>Paged project list, optionally filtered by an exact technology tag.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page (1–100).</param>
    /// <param name="tag">Optional exact technology label, e.g. <c>ASP.NET Core</c>.</param>
    /// <param name="cancellationToken">Propagates request cancellation.</param>
    /// <response code="200">Returns the page.</response>
    /// <response code="422">Paging values are out of range.</response>
    [HttpGet(Name = nameof(GetProjects))]
    [ProducesResponseType(typeof(PagedResult<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetProjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? tag = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _projects.GetProjectsAsync(page, pageSize, tag, cancellationToken);
        return Ok(result);
    }

    /// <summary>Single project by its URL slug.</summary>
    /// <response code="200">Returns the project.</response>
    /// <response code="404">No project uses that slug.</response>
    /// <response code="422">The slug is empty.</response>
    [HttpGet("{slug}", Name = nameof(GetProjectBySlug))]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProjectDto>> GetProjectBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetProjectBySlugAsync(slug, cancellationToken);
        return Ok(project);
    }
}
