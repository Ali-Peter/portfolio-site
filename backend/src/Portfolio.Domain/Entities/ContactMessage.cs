namespace Portfolio.Domain.Entities;

/// <summary>
/// A message submitted through the public contact form.
/// </summary>
public class ContactMessage : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>True once the message has been reviewed/replied to.</summary>
    public bool IsHandled { get; set; }

    /// <summary>Hashed sender IP for abuse control without storing raw IPs.</summary>
    public string? IpHash { get; set; }
}
