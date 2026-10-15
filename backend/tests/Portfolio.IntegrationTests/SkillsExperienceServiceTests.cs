using AutoMapper;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Portfolio.Application.Mapping;
using Portfolio.Application.Services;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

/// <summary>Skills + experience ordering over seeded data (real PostgreSQL).</summary>
[Collection(PostgresCollection.Name)]
public sealed class SkillsExperienceServiceTests
{
    private static readonly IConfigurationProvider Mapping = new MapperConfiguration(
        cfg => cfg.AddProfile<PortfolioMappingProfile>(),
        NullLoggerFactory.Instance);

    private readonly PostgresFixture _fixture;

    public SkillsExperienceServiceTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Skills_ReturnCategoryThenSortOrder()
    {
        await using var db = _fixture.CreateContext();
        var service = new SkillService(db, Mapping);

        var list = await service.GetSkillsAsync(CancellationToken.None);

        Assert.NotEmpty(list);
        var ordered = list.OrderBy(s => s.Category).ThenBy(s => s.SortOrder).ToList();
        Assert.Equal(ordered.Select(s => s.Name), list.Select(s => s.Name));
    }

    [Fact]
    public async Task Experience_ReturnsSeedData()
    {
        await using var db = _fixture.CreateContext();
        var service = new ExperienceService(db, Mapping);

        var entries = await service.GetExperiencesAsync(CancellationToken.None);

        Assert.NotEmpty(entries);
    }
}
