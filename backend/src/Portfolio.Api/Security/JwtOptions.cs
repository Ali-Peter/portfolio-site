namespace Portfolio.Api.Security;

/// <summary>
/// JWT signing settings for admin tokens. The key must be ≥256 bits and comes
/// from configuration only (user-secrets locally, env vars in production).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "portfolio-api";

    public string Audience { get; set; } = "portfolio-admin";

    /// <summary>Token lifetime in minutes (default 12h — single admin, low churn).</summary>
    public int ExpiryMinutes { get; set; } = 720;
}
