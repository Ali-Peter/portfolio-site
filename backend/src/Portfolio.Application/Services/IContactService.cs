using Portfolio.Application.Dto;

namespace Portfolio.Application.Services;

/// <summary>Public contact form submission.</summary>
public interface IContactService
{
    Task<ContactMessageAcceptedDto> SubmitAsync(
        CreateContactMessageRequest request,
        string? clientIp,
        CancellationToken cancellationToken = default);
}
