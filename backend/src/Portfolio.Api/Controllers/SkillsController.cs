using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers;

/// <summary>Public skill badges.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/skills")]
[Produces("application/json")]
public sealed class SkillsController : ControllerBase
{
    private readonly ISkillService _skills;

    public SkillsController(ISkillService skills)
    {
        _skills = skills;
    }

    /// <summary>All skills ordered by category, then display order.</summary>
    /// <response code="200">Returns the skill list.</response>
    [HttpGet(Name = nameof(GetSkills))]
    [ProducesResponseType(typeof(IReadOnlyList<SkillDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SkillDto>>> GetSkills(
        CancellationToken cancellationToken = default)
    {
        var skills = await _skills.GetSkillsAsync(cancellationToken);
        return Ok(skills);
    }
}
