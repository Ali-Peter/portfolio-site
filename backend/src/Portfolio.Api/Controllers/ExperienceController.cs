using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers;

/// <summary>Public resume/experience entries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/experience")]
[Produces("application/json")]
public sealed class ExperienceController : ControllerBase
{
    private readonly IExperienceService _experience;

    public ExperienceController(IExperienceService experience)
    {
        _experience = experience;
    }

    /// <summary>All experience entries in display order.</summary>
    /// <response code="200">Returns the experience list.</response>
    [HttpGet(Name = nameof(GetExperience))]
    [ProducesResponseType(typeof(IReadOnlyList<ExperienceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ExperienceDto>>> GetExperience(
        CancellationToken cancellationToken = default)
    {
        var entries = await _experience.GetExperiencesAsync(cancellationToken);
        return Ok(entries);
    }
}
