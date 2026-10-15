using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>Admin contact inbox: paged list, mark handled, delete.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/inbox")]
[Produces("application/json")]
public sealed class AdminInboxController : AdminControllerBase
{
    private readonly IAdminContentService _content;

    public AdminInboxController(
        IAdminContentService content,
        IServiceProvider services,
        ILogger<AdminInboxController> logger)
        : base(services, logger)
    {
        _content = content;
    }

    /// <summary>Paged inbox, newest first, optionally filtered by handled state.</summary>
    /// <response code="200">Returns the page.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="422">Paging values are out of range.</response>
    [HttpGet(Name = "AdminGetInbox")]
    [ProducesResponseType(typeof(PagedResult<AdminContactMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PagedResult<AdminContactMessageDto>>> GetInbox(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? handled = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _content.GetContactMessagesAsync(page, pageSize, handled, cancellationToken);
        return Ok(result);
    }

    /// <summary>Mark a message handled (or unhandled).</summary>
    /// <response code="204">Message updated.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No message uses that id.</response>
    [HttpPatch("{id:guid}/handled", Name = "AdminMarkHandled")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkHandled(
        Guid id, [FromBody] MarkHandledRequest request, CancellationToken cancellationToken = default)
    {
        await _content.MarkContactHandledAsync(id, request.Handled, cancellationToken);
        return NoContent();
    }

    /// <summary>Soft-delete a message.</summary>
    /// <response code="204">Message deleted.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    /// <response code="404">No message uses that id.</response>
    [HttpDelete("{id:guid}", Name = "AdminDeleteInboxMessage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMessage(Guid id, CancellationToken cancellationToken = default)
    {
        await _content.DeleteContactMessageAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Mark-handled body (PATCH needs an object, not a bare boolean).</summary>
    public sealed record MarkHandledRequest(bool Handled);
}
