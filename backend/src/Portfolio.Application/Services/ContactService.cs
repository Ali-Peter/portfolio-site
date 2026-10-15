using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using FluentValidation;
using Portfolio.Application.Dto;
using Portfolio.Application.Persistence;
using Portfolio.Domain.Entities;

namespace Portfolio.Application.Services;

/// <summary>
/// Contact form intake. FluentValidation runs first; a filled honeypot is
/// silently accepted (fresh 201, no persistence) so bots learn nothing.
/// </summary>
public sealed class ContactService : IContactService
{
    private readonly IPortfolioDbContext _db;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateContactMessageRequest> _validator;

    public ContactService(
        IPortfolioDbContext db,
        IMapper mapper,
        IValidator<CreateContactMessageRequest> validator)
    {
        _db = db;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ContactMessageAcceptedDto> SubmitAsync(
        CreateContactMessageRequest request,
        string? clientIp,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Bots fill the honeypot: acknowledge without storing anything.
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return new ContactMessageAcceptedDto(Guid.NewGuid(), DateTime.UtcNow);
        }

        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = _mapper.Map<ContactMessage>(request);
        entity.CreatedBy = "public-contact";
        entity.IpHash = HashIp(clientIp);

        _db.ContactMessages.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new ContactMessageAcceptedDto(entity.Id, entity.CreatedAtUtc);
    }

    /// <summary>SHA-256 of the IP + fixed purpose string; raw IPs are never stored.</summary>
    internal static string? HashIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return null;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"portfolio-contact|{ip.Trim()}"));
        return Convert.ToHexStringLower(bytes);
    }
}
