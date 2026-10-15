namespace Portfolio.Application.Exceptions;

/// <summary>
/// Thrown for semantic validation failures (bad paging, invalid query values).
/// The API middleware translates this to a 422 RFC 9457 ProblemDetails response
/// with an <c>errors</c> extension. Request-body validation uses FluentValidation
/// directly; its <see cref="FluentValidation.ValidationException"/> is mapped the same way.
/// </summary>
public sealed class PortfolioValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public PortfolioValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public PortfolioValidationException(string field, string message)
        : this(new Dictionary<string, string[]>(StringComparer.Ordinal) { [field] = [message] })
    {
    }
}
