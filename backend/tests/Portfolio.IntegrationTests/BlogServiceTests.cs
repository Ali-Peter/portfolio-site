using AutoMapper;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Portfolio.Application.Mapping;
using Portfolio.Application.Services;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

/// <summary>Slug lookup + search over seeded posts (real PostgreSQL).</summary>
[Collection(PostgresCollection.Name)]
public sealed class BlogServiceTests
{
    private static readonly IConfigurationProvider Mapping = new MapperConfiguration(
        cfg => cfg.AddProfile<PortfolioMappingProfile>(),
        NullLoggerFactory.Instance);

    private readonly PostgresFixture _fixture;

    public BlogServiceTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private PortfolioDbContext CreateContext() => _fixture.CreateContext();

    [Fact]
    public async Task GetPosts_OnlyPublished()
    {
        await using var db = CreateContext();
        var service = new BlogService(db, Mapping);

        var all = await service.GetPostsAsync(1, 20, null, CancellationToken.None);

        Assert.NotEmpty(all.Items);
        Assert.All(all.Items, p => Assert.True(p.PublishedAt.HasValue));
    }

    [Fact]
    public async Task GetPostBySlug_Seeded_ReturnsPost_Unknown_ThrowsNotFound()
    {
        await using var db = CreateContext();
        var service = new BlogService(db, Mapping);

        var all = await service.GetPostsAsync(1, 1, null, CancellationToken.None);
        var seeded = Assert.Single(all.Items);

        var bySlug = await service.GetPostBySlugAsync(seeded.Slug, CancellationToken.None);
        Assert.Equal(seeded.Title, bySlug.Title);

        await Assert.ThrowsAsync<Application.Exceptions.NotFoundException>(
            () => service.GetPostBySlugAsync("no-such-post", CancellationToken.None));
    }
}
