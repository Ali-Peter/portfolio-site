using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Dto;
using Portfolio.Application.Exceptions;

namespace Portfolio.Application.Services;

public sealed partial class AdminContentService
{
    // -- Blog --

    public async Task<IReadOnlyList<AdminBlogPostDto>> GetBlogPostsAsync(CancellationToken ct = default) =>
        await _db.BlogPosts.AsNoTracking().OrderByDescending(p => p.PublishedAt)
            .Select(p => new AdminBlogPostDto(
                p.Id, p.Slug, p.Title, p.Excerpt, p.BodyMarkdown, p.CoverImageUrl,
                p.PublishedAt, p.ReadingMinutes, p.ViewCount, p.IsPublished, p.Tags))
            .ToListAsync(ct);

    public async Task<AdminBlogPostDto> GetBlogPostAsync(Guid id, CancellationToken ct = default) =>
        await _db.BlogPosts.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new AdminBlogPostDto(
                p.Id, p.Slug, p.Title, p.Excerpt, p.BodyMarkdown, p.CoverImageUrl,
                p.PublishedAt, p.ReadingMinutes, p.ViewCount, p.IsPublished, p.Tags))
            .SingleOrDefaultAsync(ct)
        ?? throw new NotFoundException("Blog post", id.ToString());

    public async Task<AdminBlogPostDto> CreateBlogPostAsync(
        UpsertBlogPostRequest request, string actor, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var slug = NormalizeSlug(request.Slug);

        if (await _db.BlogPosts.AnyAsync(p => p.Slug == slug, ct))
        {
            throw new PortfolioValidationException("slug", "A blog post with this slug already exists.");
        }

        var entity = new Domain.Entities.BlogPost
        {
            Slug = slug,
            Title = request.Title.Trim(),
            Excerpt = request.Excerpt.Trim(),
            BodyMarkdown = request.BodyMarkdown,
            CoverImageUrl = NullIfBlank(request.CoverImageUrl),
            PublishedAt = request.PublishedAt,
            ReadingMinutes = request.ReadingMinutes,
            IsPublished = request.IsPublished,
            Tags = CleanTags(request.Tags),
            CreatedBy = actor,
        };

        _db.BlogPosts.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetBlogPostAsync(entity.Id, ct);
    }

    public async Task<AdminBlogPostDto> UpdateBlogPostAsync(
        Guid id, UpsertBlogPostRequest request, string actor, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await _db.BlogPosts.SingleOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Blog post", id.ToString());

        var slug = NormalizeSlug(request.Slug);
        if (await _db.BlogPosts.AnyAsync(p => p.Id != id && p.Slug == slug, ct))
        {
            throw new PortfolioValidationException("slug", "A blog post with this slug already exists.");
        }

        entity.Slug = slug;
        entity.Title = request.Title.Trim();
        entity.Excerpt = request.Excerpt.Trim();
        entity.BodyMarkdown = request.BodyMarkdown;
        entity.CoverImageUrl = NullIfBlank(request.CoverImageUrl);
        entity.PublishedAt = request.PublishedAt;
        entity.ReadingMinutes = request.ReadingMinutes;
        entity.IsPublished = request.IsPublished;
        entity.Tags = CleanTags(request.Tags);
        entity.UpdatedBy = actor;

        await _db.SaveChangesAsync(ct);
        return await GetBlogPostAsync(id, ct);
    }

    public async Task DeleteBlogPostAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.BlogPosts.SingleOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Blog post", id.ToString());

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }
}
