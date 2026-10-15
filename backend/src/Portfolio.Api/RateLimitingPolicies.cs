namespace Portfolio.Api;

/// <summary>
/// Rate limiter policy names. A global fixed-window policy protects every
/// endpoint; the contact and AI assistant endpoints get additional,
/// stricter per-IP policies.
/// </summary>
public static class RateLimitingPolicies
{
    public const string Global = "global";

    public const string Contact = "contact";

    public const string Ai = "ai";
}
