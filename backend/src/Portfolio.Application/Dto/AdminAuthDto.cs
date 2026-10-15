namespace Portfolio.Application.Dto;

/// <summary>Admin login request (public — rate limited at the controller).</summary>
public sealed record AdminLoginRequest(string Email, string Password);

/// <summary>Admin login response: bearer token for the Authorization header.</summary>
public sealed record AdminLoginResponse(string Token, DateTime ExpiresAtUtc);