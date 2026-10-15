namespace Portfolio.Api.Security;

/// <summary>
/// Single-admin credentials. Email + PBKDF2 password hash come from
/// configuration only (user-secrets locally, env vars in production) —
/// never the repository. <c>Password</c> is a Development-only escape hatch:
/// when set, it is hashed in-memory for comparison and a startup warning is logged.
/// </summary>
public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    public string Email { get; set; } = string.Empty;

    /// <summary>PBKDF2 hash in <c>v1.{iterations}.{salt}.{hash}</c> format.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Development-only plaintext fallback. Production ignores it unless no
    /// hash is configured (and logs a warning when it does).
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
