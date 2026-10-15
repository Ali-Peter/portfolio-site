using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;

namespace Portfolio.Api.Controllers;

/// <summary>
/// Public RAG assistant over the portfolio content. Answers stream back as
/// Server-Sent Events: a <c>sources</c> frame first (retrieved context), then
/// <c>delta</c> frames with answer text, then a terminal <c>done</c> frame.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai")]
public sealed class AiController : ControllerBase
{
    // Manual frame serialization: MVC's configured options don't reach
    // hand-rolled SSE writes, so use the web defaults (camelCase) explicitly.
    private static readonly JsonSerializerOptions FrameJson = new(JsonSerializerDefaults.Web);

    private readonly IChatService _chat;

    public AiController(IChatService chat)
    {
        _chat = chat;
    }

    /// <summary>
    /// Ask a question about the portfolio. The answer is generated strictly
    /// from retrieved portfolio content and streamed as SSE.
    /// </summary>
    /// <response code="200">SSE stream: <c>sources</c> → <c>delta</c>… → <c>done</c>.</response>
    /// <response code="422">The question is empty or too long.</response>
    /// <response code="429">Rate limit exceeded (stricter policy than the public GETs).</response>
    /// <response code="503">The assistant is not configured on this server.</response>
    [HttpPost("assistant", Name = nameof(Assistant))]
    [EnableRateLimiting(RateLimitingPolicies.Ai)]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task Assistant(
        [FromBody] CreateChatRequest request,
        CancellationToken cancellationToken = default)
    {
        // No caching or proxy buffering for event streams.
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache, no-store";
        Response.Headers["X-Accel-Buffering"] = "no";

        await using var ms = new MemoryStream();
        await using var writer = new StreamWriter(ms) { AutoFlush = true };

        await foreach (var frame in _chat.AskAsync(request, cancellationToken))
        {
            var line = $"data: {JsonSerializer.Serialize(frame, FrameJson)}";
            await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
            await writer.WriteLineAsync(string.Empty.AsMemory(), cancellationToken);
        }

        // Flush into a local buffer so we never write synchronously to the
        // Kestrel response stream (which throws in a sync context).
        ms.Position = 0;
        await ms.CopyToAsync(Response.Body, cancellationToken);
    }
}