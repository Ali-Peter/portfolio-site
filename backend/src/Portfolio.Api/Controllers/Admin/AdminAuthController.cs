using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>Single-admin login. Issues a JWT bearer token for /admin/* routes.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/auth")]
[Produces("application/json")]
public sealed class AdminAuthController : ControllerBase
{
    private readonly IAdminAuthService _auth;

    public AdminAuthController(IAdminAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Log in with the configured admin email + password.</summary>
    /// <response code="200">Returns the bearer token and expiry.</response>
    /// <response code="401">Invalid email or password.</response>
    /// <response code="422">Request validation failed.</response>
    /// <response code="429">Too many login attempts (10/min per IP).</response>
    [HttpPost("login", Name = "AdminLogin")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.AdminAuth)]
    [ProducesResponseType(typeof(AdminLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AdminLoginResponse>> Login(
        [FromBody] AdminLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _auth.LoginAsync(request, cancellationToken);
        return Ok(response);
    }
}
