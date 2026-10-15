namespace Portfolio.Application.Exceptions;

/// <summary>
/// Thrown for bad admin credentials. The API middleware translates this to a
/// 401 RFC 9457 ProblemDetails response with a generic message (no hint about
/// which field was wrong).
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
