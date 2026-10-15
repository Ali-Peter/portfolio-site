using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>Manual RAG reindex trigger (mutations already queue one automatically).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/reindex")]
[Produces("application/json")]
public sealed class AdminReindexController : AdminControllerBase
{
    public AdminReindexController(IServiceProvider services, ILogger<AdminReindexController> logger)
        : base(services, logger)
    {
    }

    /// <summary>
    /// Rebuild the RAG index now and wait for it. Returns the chunk count
    /// (0 when no embedding provider is configured — skipped, not an error).
    /// </summary>
    /// <response code="200">Returns the indexed chunk count.</response>
    /// <response code="401">Missing or invalid admin token.</response>
    [HttpPost(Name = "AdminReindex")]
    [ProducesResponseType(typeof(ReindexResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReindexResult>> Reindex(CancellationToken cancellationToken = default)
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var indexer = scope.ServiceProvider.GetRequiredService<IPortfolioIndexer>();
        var count = await indexer.ReindexAsync(cancellationToken);
        return Ok(new ReindexResult(count));
    }

    /// <summary>Indexed chunk count from a reindex run.</summary>
    public sealed record ReindexResult(int Chunks);
}
