using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Dto;
using Portfolio.Application.Exceptions;

namespace Portfolio.Application.Services;

public sealed partial class AdminContentService
{
    // -- Skills --

    public async Task<IReadOnlyList<AdminSkillDto>> GetSkillsAsync(CancellationToken ct = default) =>
        await _db.Skills.AsNoTracking().OrderBy(s => s.SortOrder)
            .Select(s => new AdminSkillDto(s.Id, s.Name, s.Category, s.Proficiency, s.SortOrder))
            .ToListAsync(ct);

    public async Task<AdminSkillDto> GetSkillAsync(Guid id, CancellationToken ct = default) =>
        await _db.Skills.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new AdminSkillDto(s.Id, s.Name, s.Category, s.Proficiency, s.SortOrder))
            .SingleOrDefaultAsync(ct)
        ?? throw new NotFoundException("Skill", id.ToString());

    public async Task<AdminSkillDto> CreateSkillAsync(
        UpsertSkillRequest request, string actor, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var name = request.Name.Trim();

        if (await _db.Skills.AnyAsync(s => s.Name == name, ct))
        {
            throw new PortfolioValidationException("name", $"A skill named '{name}' already exists.");
        }

        var entity = new Domain.Entities.Skill
        {
            Name = name,
            Category = request.Category,
            Proficiency = request.Proficiency,
            SortOrder = request.SortOrder,
            CreatedBy = actor,
        };

        _db.Skills.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetSkillAsync(entity.Id, ct);
    }

    public async Task<AdminSkillDto> UpdateSkillAsync(
        Guid id, UpsertSkillRequest request, string actor, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await _db.Skills.SingleOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Skill", id.ToString());

        var name = request.Name.Trim();
        if (await _db.Skills.AnyAsync(s => s.Id != id && s.Name == name, ct))
        {
            throw new PortfolioValidationException("name", $"A skill named '{name}' already exists.");
        }

        entity.Name = name;
        entity.Category = request.Category;
        entity.Proficiency = request.Proficiency;
        entity.SortOrder = request.SortOrder;
        entity.UpdatedBy = actor;

        await _db.SaveChangesAsync(ct);
        return await GetSkillAsync(id, ct);
    }

    public async Task DeleteSkillAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Skills.SingleOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Skill", id.ToString());

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    // -- Experience --

    public async Task<IReadOnlyList<AdminExperienceDto>> GetExperiencesAsync(CancellationToken ct = default) =>
        await _db.Experiences.AsNoTracking().OrderBy(e => e.SortOrder)
            .Select(e => new AdminExperienceDto(
                e.Id, e.Company, e.Title, e.Description, e.Technologies,
                e.StartedOn, e.EndedOn, e.IsCurrent, e.SortOrder))
            .ToListAsync(ct);

    public async Task<AdminExperienceDto> GetExperienceAsync(Guid id, CancellationToken ct = default) =>
        await _db.Experiences.AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new AdminExperienceDto(
                e.Id, e.Company, e.Title, e.Description, e.Technologies,
                e.StartedOn, e.EndedOn, e.IsCurrent, e.SortOrder))
            .SingleOrDefaultAsync(ct)
        ?? throw new NotFoundException("Experience", id.ToString());

    public async Task<AdminExperienceDto> CreateExperienceAsync(
        UpsertExperienceRequest request, string actor, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = new Domain.Entities.Experience
        {
            Company = request.Company.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description,
            Technologies = CleanTags(request.Technologies),
            StartedOn = request.StartedOn,
            EndedOn = request.EndedOn,
            IsCurrent = request.IsCurrent,
            SortOrder = request.SortOrder,
            CreatedBy = actor,
        };

        _db.Experiences.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetExperienceAsync(entity.Id, ct);
    }

    public async Task<AdminExperienceDto> UpdateExperienceAsync(
        Guid id, UpsertExperienceRequest request, string actor, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await _db.Experiences.SingleOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new NotFoundException("Experience", id.ToString());

        entity.Company = request.Company.Trim();
        entity.Title = request.Title.Trim();
        entity.Description = request.Description;
        entity.Technologies = CleanTags(request.Technologies);
        entity.StartedOn = request.StartedOn;
        entity.EndedOn = request.EndedOn;
        entity.IsCurrent = request.IsCurrent;
        entity.SortOrder = request.SortOrder;
        entity.UpdatedBy = actor;

        await _db.SaveChangesAsync(ct);
        return await GetExperienceAsync(id, ct);
    }

    public async Task DeleteExperienceAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Experiences.SingleOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new NotFoundException("Experience", id.ToString());

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }
}
