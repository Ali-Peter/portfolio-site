namespace Portfolio.Application.Exceptions;

/// <summary>
/// Thrown when the AI assistant cannot serve a request for infrastructure
/// reasons (no API key configured, upstream provider unreachable state).
/// The API middleware translates this to a 503 RFC 9457 ProblemDetails response.
/// </summary>
public sealed class AiUnavailableException : Exception
{
    public AiUnavailableException(string message)
        : base(message)
    {
    }
}