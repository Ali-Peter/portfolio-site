using Portfolio.Application.Dto;

namespace Portfolio.Application.Services;

/// <summary>Single-admin credential check + token issuance contract.</summary>
public interface IAdminAuthService
{
    /// <summary>
    /// Validate credentials and return a signed token.
    /// Throws <see cref="Exceptions.UnauthorizedException"/> on bad credentials.
    /// </summary>
    Task<AdminLoginResponse> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default);
}
