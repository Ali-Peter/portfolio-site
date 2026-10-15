namespace Portfolio.Application.Dto;

/// <summary>Contact form submission sent by the public site (request model).</summary>
public sealed record CreateContactMessageRequest(
    string Name,
    string Email,
    string Message,
    /// <summary>
    /// Honeypot field: the real form never fills this in. Any non-empty value
    /// means a bot submitted the form and the request is silently accepted
    /// (201) without persisting anything.
    /// </summary>
    string? Website);
