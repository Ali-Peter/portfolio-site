using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common;
using Portfolio.Application.Dto;
using Portfolio.Application.Exceptions;
using Portfolio.Application.Persistence;

namespace Portfolio.Application.Services;

/// <summary>Read-only project queries; mapping runs in the database (no client evaluation).</summary>
public sealed class ProjectService : IProjectService
{
    private readonly IPortfolioDbContext _db;
    private readonly IConfigurationProvider _mapping;

    public ProjectService(IPortfolioDbContext db, IConfigurationProvider mapping)
    {
        _db = db;
        _mapping = mapping;
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsAsync(
        int page,
        int pageSize,
        string? tag,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = Pagination.Validate(page, pageSize);

        var query = _db.Projects.AsNoTracking().OrderBy(p => p.SortOrder);

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var normalized = tag.Trim();
            query = (IOrderedQueryable<Domain.Entities.Project>)query.Where(p => p.Technologies.Any(t => t == normalized));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ProjectDto>(_mapping)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectDto>(
            items,
            page,
            pageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<ProjectDto> GetProjectBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new PortfolioValidationException("slug", "Slug must not be empty.");
        }

        var normalized = slug.Trim().ToLowerInvariant();

        var dto = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Slug == normalized)
            .ProjectTo<ProjectDto>(_mapping)
            .SingleOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException("Project", slug.Trim());
    }
}
