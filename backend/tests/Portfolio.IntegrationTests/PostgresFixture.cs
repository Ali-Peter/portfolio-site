using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Portfolio.IntegrationTests;

/// <summary>
/// Shared PostgreSQL 16 container: one container for the whole collection,
/// migrated + seeded once. Requires a running Docker daemon.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .WithDatabase("portfolio_test")
        .WithUsername("portfolio")
        .WithPassword("portfolio_test_only")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Context options identical to the application wiring (snake-case naming included).
    /// Centralized here so tests can never drift from the real configuration again.
    /// </summary>
    public PortfolioDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseNpgsql(ConnectionString, npgsql => npgsql.UseVector())
            .UseSnakeCaseNamingConvention()
            .Options;
        return new PortfolioDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
        await SeedData.RunAsync(db);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
