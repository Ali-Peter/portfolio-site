using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Dto;
using Portfolio.Application.Exceptions;

namespace Portfolio.Application.Services;

public sealed partial class AdminContentService
{
    // -- Projects --

    public async Task<IReadOnlyList<AdminProjectDto>> GetProjectsAsync(CancellationToken ct = default) =>
        await _db.Projects.AsNoTracking().OrderBy(p => p.SortOrder)
            .Select(p => new AdminProjectDto(
                p.Id, p.Slug, p.Title, p.Summary, p.Description, p.RepoUrl, p.DemoUrl,
                p.CoverImageUrl, p.Status, p.Featured, p.StartedOn, p.CompletedOn,
                p.SortOrder, p.Technologies))
            .ToListAsync(ct);

    public async Task<AdminProjectDto> GetProjectAsync(Guid id, CancellationToken ct = default) =>
        await _db.Projects.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new AdminProjectDto(
                p.Id, p.Slug, p.Title, p.Summary, p.Description, p.RepoUrl, p.DemoUrl,
                p.CoverImageUrl, p.Status, p.Featured, p.StartedOn, p.CompletedOn,
                p.SortOrder, p.Technologies))
            .SingleOrDefaultAsync(ct)
        ?? throw new NotFoundException("Project", id.ToString());

    public async Task<AdminProjectDto> CreateProjectAsync(
        UpsertProjectRequest request, string actor, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var slug = NormalizeSlug(request.Slug);

        if (await _db.Projects.AnyAsync(p => p.Slug == slug, ct))
        {
            throw new PortfolioValidationException("slug", $"A project with slug '{slug}' already exists.");
        }

        var entity = new Domain.Entities.Project
        {
            Slug = slug,
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Description = request.Description,
            RepoUrl = NullIfBlank(request.RepoUrl),
            DemoUrl = NullIfBlank(request.DemoUrl),
            CoverImageUrl = NullIfBlank(request.CoverImageUrl),
            Status = request.Status,
            Featured = request.Featured,
            StartedOn = request.StartedOn,
            CompletedOn = request.CompletedOn,
            SortOrder = request.SortOrder,
            Technologies = CleanTags(request.Technologies),
            CreatedBy = actor,
        };

        _db.Projects.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetProjectAsync(entity.Id, ct);
    }

    public async Task<AdminProjectDto> UpdateProjectAsync(
        Guid id, UpsertProjectRequest request, string actor, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await _db.Projects.SingleOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Project", id.ToString());

        var slug = NormalizeSlug(request.Slug);
        if (await _db.Projects.AnyAsync(p => p.Id != id && p.Slug == slug, ct))
        {
            throw new PortfolioValidationException("slug", $"A project with slug '{slug}' already exists.");
        }

        entity.Slug = slug;
        entity.Title = request.Title.Trim();
        entity.Summary = request.Summary.Trim();
        entity.Description = request.Description;
        entity.RepoUrl = NullIfBlank(request.RepoUrl);
        entity.DemoUrl = NullIfBlank(request.DemoUrl);
        entity.CoverImageUrl = NullIfBlank(request.CoverImageUrl);
        entity.Status = request.Status;
        entity.Featured = request.Featured;
        entity.StartedOn = request.StartedOn;
        entity.CompletedOn = request.CompletedOn;
        entity.SortOrder = request.SortOrder;
        entity.Technologies = CleanTags(request.Technologies);
        entity.UpdatedBy = actor;

        await _db.SaveChangesAsync(ct);
        return await GetProjectAsync(id, ct);
    }

    public async Task DeleteProjectAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Projects.SingleOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Project", id.ToString());

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }
}
