using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>Admin experience CRUD (soft-delete on remove).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/experience")]
[Produces("application/json")]
public sealed class AdminExperienceController : AdminControllerBase
{
    private readonly IAdminContentService _content;
    private readonly IValidator<UpsertExperienceRequest> _validator;

    public AdminExperienceController(
        IAdminContentService content,
        IValidator<UpsertExperienceRequest> validator,
        IServiceProvider services,
        ILogger<AdminExperienceController> logger)
        : base(services, logger)
    {
        _content = content;
        _validator = validator;
    }

    /// <summary>All experience entries for the admin list.</summary>
    /// <response code="200">Returns the admin experience rows.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    [HttpGet(Name = "AdminGetExperiences")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminExperienceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AdminExperienceDto>>> GetExperiences(
        CancellationToken cancellationToken = default)
    {
        var items = await _content.GetExperiencesAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>Single experience entry for the admin editor.</summary>
    /// <response code="200">Returns the entry.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No entry uses that id.</response>
    [HttpGet("{id:guid}", Name = "AdminGetExperience")]
    [ProducesResponseType(typeof(AdminExperienceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminExperienceDto>> GetExperience(
        Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _content.GetExperienceAsync(id, cancellationToken);
        return Ok(entry);
    }

    /// <summary>Create an experience entry.</summary>
    /// <response code="201">Returns the created entry.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost(Name = "AdminCreateExperience")]
    [ProducesResponseType(typeof(AdminExperienceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AdminExperienceDto>> CreateExperience(
        [FromBody] UpsertExperienceRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var created = await _content.CreateExperienceAsync(request, Actor, cancellationToken);
        QueueReindex();
        return CreatedAtRoute(nameof(GetExperience), new { id = created.Id, version = "1" }, created);
    }

    /// <summary>Replace an experience entry.</summary>
    /// <response code="200">Returns the updated entry.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No entry uses that id.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPut("{id:guid}", Name = "AdminUpdateExperience")]
    [ProducesResponseType(typeof(AdminExperienceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AdminExperienceDto>> UpdateExperience(
        Guid id, [FromBody] UpsertExperienceRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var updated = await _content.UpdateExperienceAsync(id, request, Actor, cancellationToken);
        QueueReindex();
        return Ok(updated);
    }

    /// <summary>Soft-delete an experience entry.</summary>
    /// <response code="204">Entry deleted.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No entry uses that id.</response>
    [HttpDelete("{id:guid}", Name = "AdminDeleteExperience")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExperience(Guid id, CancellationToken cancellationToken = default)
    {
        await _content.DeleteExperienceAsync(id, cancellationToken);
        QueueReindex();
        return NoContent();
    }
}
