using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>
/// Base class for admin controllers: [Authorize] + a fire-and-forget RAG
/// reindex helper. Content mutations call <c>QueueReindex()</c> so the public
/// chatbot reflects edits without blocking the response (embeddings are
/// batched inside the indexer; failures only log).
/// </summary>
[Authorize]
public abstract class AdminControllerBase : ControllerBase
{
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;

    protected AdminControllerBase(IServiceProvider services, ILogger logger)
    {
        _services = services;
        _logger = logger;
    }

    /// <summary>Actor label stamped into CreatedBy/UpdatedBy audit fields.</summary>
    protected string Actor =>
        User.Identity?.Name ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "admin";

    /// <summary>
    /// Rebuild the RAG index in the background after a content mutation.
    /// Skips itself (with a log line) when no OpenAI key is configured.
    /// </summary>
    protected void QueueReindex()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _services.CreateScope();
                var indexer = scope.ServiceProvider.GetRequiredService<IPortfolioIndexer>();
                await indexer.ReindexAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background admin reindex failed.");
            }
        });
    }
}
