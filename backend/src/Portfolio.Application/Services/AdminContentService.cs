using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common;
using Portfolio.Application.Dto;
using Portfolio.Application.Exceptions;
using Portfolio.Application.Persistence;

namespace Portfolio.Application.Services;

/// <summary>
/// Admin content management over the portfolio entities. Reads include
/// drafts/unpublished rows; deletes are soft (IsDeleted) so history survives.
/// Slug/name conflicts throw <see cref="PortfolioValidationException"/> (422),
/// unknown ids throw <see cref="NotFoundException"/> (404).
/// Split into partial files per content type.
/// </summary>
public sealed partial class AdminContentService : IAdminContentService
{
    private readonly IPortfolioDbContext _db;

    public AdminContentService(IPortfolioDbContext db)
    {
        _db = db;
    }

    // -- Contact inbox --

    public async Task<PagedResult<AdminContactMessageDto>> GetContactMessagesAsync(
        int page, int pageSize, bool? handled, CancellationToken ct = default)
    {
        (page, pageSize) = Pagination.Validate(page, pageSize);

        var query = _db.ContactMessages.AsNoTracking().OrderByDescending(c => c.CreatedAtUtc);
        if (handled.HasValue)
        {
            query = (IOrderedQueryable<Domain.Entities.ContactMessage>)query.Where(c => c.IsHandled == handled.Value);
        }

        var totalCount = await query.LongCountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AdminContactMessageDto(
                c.Id, c.Name, c.Email, c.Message, c.IsHandled, c.CreatedAtUtc))
            .ToListAsync(ct);

        return new PagedResult<AdminContactMessageDto>(
            items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task MarkContactHandledAsync(Guid id, bool handled, CancellationToken ct = default)
    {
        var entity = await _db.ContactMessages.SingleOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Contact message", id.ToString());

        entity.IsHandled = handled;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteContactMessageAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.ContactMessages.SingleOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Contact message", id.ToString());

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    // -- Helpers --

    private static string NormalizeSlug(string slug) => slug.Trim().ToLowerInvariant();

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[] CleanTags(IReadOnlyList<string>? tags) =>
        tags == null ? [] : tags.Select(t => t.Trim()).Where(t => t.Length > 0).ToArray();
}
