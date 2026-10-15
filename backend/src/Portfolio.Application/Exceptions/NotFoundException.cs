namespace Portfolio.Application.Exceptions;

/// <summary>
/// Thrown when a requested resource (project, blog post, ...) does not exist.
/// The API middleware translates this to a 404 RFC 9457 ProblemDetails response.
/// </summary>
public sealed class NotFoundException : Exception
{
    public string Resource { get; }

    public string Key { get; }

    public NotFoundException(string resource, string key)
        : base($"{resource} '{key}' was not found.")
    {
        Resource = resource;
        Key = key;
    }
}
