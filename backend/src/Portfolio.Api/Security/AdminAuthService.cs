using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Application.Dto;
using Portfolio.Application.Exceptions;
using Portfolio.Application.Services;

namespace Portfolio.Api.Security;

/// <summary>
/// Single-admin credential check + JWT issuance. Email comparison is
/// case-insensitive; failures throw <see cref="UnauthorizedException"/> (401)
/// with a generic message so attackers learn nothing about which field failed.
/// </summary>
public sealed class AdminAuthService : IAdminAuthService
{
    private readonly AdminOptions _admin;
    private readonly JwtOptions _jwt;
    private readonly IValidator<AdminLoginRequest> _validator;
    private readonly ILogger<AdminAuthService> _logger;

    public AdminAuthService(
        IOptions<AdminOptions> admin,
        IOptions<JwtOptions> jwt,
        IValidator<AdminLoginRequest> validator,
        ILogger<AdminAuthService> logger)
    {
        _admin = admin.Value;
        _jwt = jwt.Value;
        _validator = validator;
        _logger = logger;
    }

    public async Task<AdminLoginResponse> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var emailMatches = string.Equals(
            request.Password == null ? null : request.Email.Trim(),
            _admin.Email.Trim(),
            StringComparison.OrdinalIgnoreCase);

        var passwordMatches = VerifyPassword(request.Password ?? string.Empty);

        if (!emailMatches || !passwordMatches)
        {
            // Generic message + warning log (no email echo) for brute-force visibility.
            _logger.LogWarning("Failed admin login attempt.");
            throw new UnauthorizedException("Invalid email or password.");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);
        var token = CreateToken(_admin.Email.Trim(), expiresAt);

        _logger.LogInformation("Admin login succeeded.");
        return new AdminLoginResponse(token, expiresAt);
    }

    private bool VerifyPassword(string password)
    {
        if (!string.IsNullOrWhiteSpace(_admin.PasswordHash))
        {
            return AdminPasswordHasher.Verify(password, _admin.PasswordHash);
        }

        // Development-only plaintext fallback (warned at startup).
        return !string.IsNullOrEmpty(_admin.Password)
            && CryptographicEquals(password, _admin.Password);
    }

    private string CreateToken(string email, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: [new Claim(ClaimTypes.Name, email), new Claim(ClaimTypes.Role, "admin")],
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool CryptographicEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}
