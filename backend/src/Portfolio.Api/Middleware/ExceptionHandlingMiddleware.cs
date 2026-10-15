using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Exceptions;

namespace Portfolio.Api.Middleware;

/// <summary>
/// Global exception → RFC 9457 ProblemDetails translation.
/// - <see cref="NotFoundException"/> → 404
/// - <see cref="PortfolioValidationException"/> / FluentValidation
///   <see cref="ValidationException"/> → 422 with an <c>errors</c> extension
/// - <see cref="UnauthorizedException"/> → 401 (generic message, no hints)
/// - everything else → 500 with no internal details leaked.
/// The middleware also stamps the <c>traceId</c> extension; the global
/// <c>UseExceptionHandler</c> problem-details writer below it handles framework
/// exceptions (e.g. rate-limiter rejections are written by their own middleware).
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        // The response may already have started (e.g. streaming); nothing safe to do.
        if (context.Response.HasStarted)
        {
            _logger.LogError(ex, "Unhandled exception after response started. TraceId: {TraceId}", context.TraceIdentifier);
            throw ex;
        }

        var (status, title, errors) = ex switch
        {
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                (IReadOnlyDictionary<string, string[]>?)new Dictionary<string, string[]>(
                    StringComparer.Ordinal)
                {
                    [notFound.Resource] = [$"'{notFound.Key}' was not found."],
                }),
            PortfolioValidationException validation => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                validation.Errors),
            UnauthorizedException unauthorized => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                (IReadOnlyDictionary<string, string[]>?)null),
            ValidationException fluent => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                ToErrorDictionary(fluent)),
            // Safe, user-facing message (e.g. "assistant not configured") —
            // surfaced verbatim even in production.
            AiUnavailableException ai => (
                StatusCodes.Status503ServiceUnavailable,
                "Service unavailable",
                (IReadOnlyDictionary<string, string[]>?)null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                (IReadOnlyDictionary<string, string[]>?)null),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
        }
        else
        {
            _logger.LogInformation(
                "Request rejected with {StatusCode}: {Message}. TraceId: {TraceId}",
                status,
                ex.Message,
                context.TraceIdentifier);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError && !_environment.IsDevelopment()
                ? "An unexpected error occurred. Contact support with the trace ID."
                : ex.Message,
            Instance = context.Request.Path,
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (errors is { Count: > 0 })
        {
            problem.Extensions["errors"] = errors;
        }

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken: context.RequestAborted);
    }

    private static IReadOnlyDictionary<string, string[]> ToErrorDictionary(ValidationException ex)
    {
        var grouped = ex.Errors
            .GroupBy(f => f.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        return grouped.Count > 0
            ? grouped
            : new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["request"] = [ex.Message],
            };
    }
}
