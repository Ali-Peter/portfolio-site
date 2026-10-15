using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Dto;
using Portfolio.Application.Persistence;

namespace Portfolio.Application.Services;

/// <summary>Resume/experience entries in display order.</summary>
public sealed class ExperienceService : IExperienceService
{
    private readonly IPortfolioDbContext _db;
    private readonly IConfigurationProvider _mapping;

    public ExperienceService(IPortfolioDbContext db, IConfigurationProvider mapping)
    {
        _db = db;
        _mapping = mapping;
    }

    public async Task<IReadOnlyList<ExperienceDto>> GetExperiencesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.Experiences
            .AsNoTracking()
            .OrderBy(e => e.SortOrder)
            .ProjectTo<ExperienceDto>(_mapping)
            .ToListAsync(cancellationToken);
    }
}
