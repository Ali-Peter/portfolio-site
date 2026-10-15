using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Dto;
using Portfolio.Application.Persistence;
using Portfolio.Domain.Enums;

namespace Portfolio.Application.Services;

/// <summary>
/// Skill badges, ordered by category (enum declaration order) then sort order.
/// Category is stored as text in PostgreSQL, whose alphabetical collation does
/// not match the enum order ("AiMl" &lt; "Backend"), so the ordering is expressed
/// as a translatable CASE expression rather than a plain OrderBy on the column.
/// </summary>
public sealed class SkillService : ISkillService
{
    private readonly IPortfolioDbContext _db;
    private readonly IConfigurationProvider _mapping;

    public SkillService(IPortfolioDbContext db, IConfigurationProvider mapping)
    {
        _db = db;
        _mapping = mapping;
    }

    public async Task<IReadOnlyList<SkillDto>> GetSkillsAsync(
        CancellationToken cancellationToken = default)
    {
        // EF Core translates this ternary chain to a SQL CASE expression.
        // A C# helper method would force client-side evaluation and break ProjectTo.
        return await _db.Skills
            .AsNoTracking()
            .OrderBy(s =>
                s.Category == SkillCategory.Backend ? 0
                : s.Category == SkillCategory.Frontend ? 1
                : s.Category == SkillCategory.DataAndInfra ? 2
                : s.Category == SkillCategory.AiMl ? 3
                : 4)
            .ThenBy(s => s.SortOrder)
            .ProjectTo<SkillDto>(_mapping)
            .ToListAsync(cancellationToken);
    }
}
