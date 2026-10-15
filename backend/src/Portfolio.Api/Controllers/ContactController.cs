using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers;

/// <summary>Public contact form intake.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contact")]
[Produces("application/json")]
public sealed class ContactController : ControllerBase
{
    private readonly IContactService _contact;

    public ContactController(IContactService contact)
    {
        _contact = contact;
    }

    /// <summary>
    /// Submit a contact message. A filled <c>website</c> honeypot is silently
    /// accepted (201) without being stored.
    /// </summary>
    /// <response code="201">Message accepted.</response>
    /// <response code="422">Request validation failed.</response>
    /// <response code="429">Rate limit exceeded (stricter policy than the public GETs).</response>
    [HttpPost(Name = nameof(SubmitContact))]
    [EnableRateLimiting(RateLimitingPolicies.Contact)]
    [ProducesResponseType(typeof(ContactMessageAcceptedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ContactMessageAcceptedDto>> SubmitContact(
        [FromBody] CreateContactMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var accepted = await _contact.SubmitAsync(request, clientIp, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, accepted);
    }
}
