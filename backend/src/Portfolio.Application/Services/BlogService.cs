using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common;
using Portfolio.Application.Dto;
using Portfolio.Application.Exceptions;
using Portfolio.Application.Persistence;

namespace Portfolio.Application.Services;

/// <summary>Read-only blog queries over published posts; mapping runs in the database.</summary>
public sealed class BlogService : IBlogService
{
    private readonly IPortfolioDbContext _db;
    private readonly IConfigurationProvider _mapping;

    public BlogService(IPortfolioDbContext db, IConfigurationProvider mapping)
    {
        _db = db;
        _mapping = mapping;
    }

    public async Task<PagedResult<BlogPostDto>> GetPostsAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = Pagination.Validate(page, pageSize);

        var query = _db.BlogPosts
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedAt);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = (IOrderedQueryable<Domain.Entities.BlogPost>)query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.Excerpt.ToLower().Contains(term));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<BlogPostDto>(_mapping)
            .ToListAsync(cancellationToken);

        return new PagedResult<BlogPostDto>(
            items,
            page,
            pageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<BlogPostDto> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new PortfolioValidationException("slug", "Slug must not be empty.");
        }

        var normalized = slug.Trim().ToLowerInvariant();

        var dto = await _db.BlogPosts
            .AsNoTracking()
            .Where(p => p.IsPublished && p.Slug == normalized)
            .ProjectTo<BlogPostDto>(_mapping)
            .SingleOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException("Blog post", slug.Trim());
    }
}
