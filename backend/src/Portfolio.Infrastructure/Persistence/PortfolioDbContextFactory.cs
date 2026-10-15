using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Portfolio.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so <c>dotnet ef migrations</c> works without booting the API.
/// The connection string targets the local docker-compose database only
/// (defaults documented in .env.example) — it is not a secret.
/// </summary>
public sealed class PortfolioDbContextFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
    public PortfolioDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=portfolio;Username=portfolio;Password=portfolio_local_dev",
                npgsql => npgsql.UseVector())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new PortfolioDbContext(options);
    }
}
