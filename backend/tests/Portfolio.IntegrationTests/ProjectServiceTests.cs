using AutoMapper;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Portfolio.Application.Dto;
using Portfolio.Application.Mapping;
using Portfolio.Application.Services;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

/// <summary>
/// Project queries against a real PostgreSQL 16 container (Testcontainers).
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class ProjectServiceTests
{
    private static readonly IConfigurationProvider Mapping = new MapperConfiguration(
        cfg => cfg.AddProfile<PortfolioMappingProfile>(),
        NullLoggerFactory.Instance);

    private readonly PostgresFixture _fixture;

    public ProjectServiceTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private PortfolioDbContext CreateContext() => _fixture.CreateContext();

    [Fact]
    public async Task GetProjects_PagesResults()
    {
        await using var db = CreateContext();
        var service = new ProjectService(db, Mapping);

        var page1 = await service.GetProjectsAsync(1, 2, null, CancellationToken.None);

        Assert.Equal(2, page1.Items.Count);
        Assert.True(page1.TotalCount >= 3);
        Assert.Equal(1, page1.Page);
    }

    [Fact]
    public async Task GetProjects_TagFilter_ReturnsOnlyMatching()
    {
        await using var db = CreateContext();
        var service = new ProjectService(db, Mapping);

        // "FastAPI" is a technology claimed by the seeded Outbound AI Backend project.
        var filtered = await service.GetProjectsAsync(
            1, 20, "FastAPI", CancellationToken.None);

        Assert.NotEmpty(filtered.Items);
        Assert.All(filtered.Items, p => Assert.Contains("FastAPI", p.Technologies));
    }

    [Fact]
    public async Task GetProjects_InvalidPaging_ThrowsValidation()
    {
        await using var db = CreateContext();
        var service = new ProjectService(db, Mapping);

        await Assert.ThrowsAsync<Application.Exceptions.PortfolioValidationException>(
            () => service.GetProjectsAsync(0, 0, null, CancellationToken.None));
    }
}
