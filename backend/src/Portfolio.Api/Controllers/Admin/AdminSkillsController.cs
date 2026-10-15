using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>Admin skill CRUD (soft-delete on remove).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/skills")]
[Produces("application/json")]
public sealed class AdminSkillsController : AdminControllerBase
{
    private readonly IAdminContentService _content;
    private readonly IValidator<UpsertSkillRequest> _validator;

    public AdminSkillsController(
        IAdminContentService content,
        IValidator<UpsertSkillRequest> validator,
        IServiceProvider services,
        ILogger<AdminSkillsController> logger)
        : base(services, logger)
    {
        _content = content;
        _validator = validator;
    }

    /// <summary>All skills for the admin list.</summary>
    /// <response code="200">Returns the admin skill rows.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    [HttpGet(Name = "AdminGetSkills")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminSkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AdminSkillDto>>> GetSkills(
        CancellationToken cancellationToken = default)
    {
        var items = await _content.GetSkillsAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>Single skill for the admin editor.</summary>
    /// <response code="200">Returns the skill.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No skill uses that id.</response>
    [HttpGet("{id:guid}", Name = "AdminGetSkill")]
    [ProducesResponseType(typeof(AdminSkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminSkillDto>> GetSkill(
        Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await _content.GetSkillAsync(id, cancellationToken);
        return Ok(skill);
    }

    /// <summary>Create a skill.</summary>
    /// <response code="201">Returns the created skill.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="422">Request validation failed (incl. duplicate name).</response>
    [HttpPost(Name = "AdminCreateSkill")]
    [ProducesResponseType(typeof(AdminSkillDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AdminSkillDto>> CreateSkill(
        [FromBody] UpsertSkillRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var created = await _content.CreateSkillAsync(request, Actor, cancellationToken);
        QueueReindex();
        return CreatedAtRoute(nameof(GetSkill), new { id = created.Id, version = "1" }, created);
    }

    /// <summary>Replace a skill.</summary>
    /// <response code="200">Returns the updated skill.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No skill uses that id.</response>
    /// <response code="422">Request validation failed (incl. duplicate name).</response>
    [HttpPut("{id:guid}", Name = "AdminUpdateSkill")]
    [ProducesResponseType(typeof(AdminSkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AdminSkillDto>> UpdateSkill(
        Guid id, [FromBody] UpsertSkillRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var updated = await _content.UpdateSkillAsync(id, request, Actor, cancellationToken);
        QueueReindex();
        return Ok(updated);
    }

    /// <summary>Soft-delete a skill.</summary>
    /// <response code="204">Skill deleted.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No skill uses that id.</response>
    [HttpDelete("{id:guid}", Name = "AdminDeleteSkill")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSkill(Guid id, CancellationToken cancellationToken = default)
    {
        await _content.DeleteSkillAsync(id, cancellationToken);
        QueueReindex();
        return NoContent();
    }
}
