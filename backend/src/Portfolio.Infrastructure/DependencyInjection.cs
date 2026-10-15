using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Portfolio.Application.Services;
using Portfolio.Infrastructure.Ai;
using Portfolio.Infrastructure.Health;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    public const string ConnectionStringName = "PortfolioDb";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. " +
                "Set it in appsettings.Development.json (local) or via the " +
                "ConnectionStrings__PortfolioDb environment variable (production).");

        services.AddDbContext<PortfolioDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                    // pgvector type support (Vector ↔ vector columns).
                    npgsql.UseVector();
                    // History table intentionally left at the EF default
                    // (__EFMigrationsHistory) so runtime MigrateAsync and the
                    // design-time factory used by `dotnet ef` see the same state.
                })
                .UseSnakeCaseNamingConvention());

        // Application services depend on the abstraction, not the concrete context.
        services.AddScoped<Application.Persistence.IPortfolioDbContext>(
            sp => sp.GetRequiredService<PortfolioDbContext>());

        // Self-contained PostgreSQL health check (Npgsql only, no extra package).
        services.AddSingleton<IHealthCheck, PostgresHealthCheck>();

        // OpenAI provider clients (typed HttpClients). The API key is read from
        // configuration only: user-secrets locally, environment in production.
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.AddHttpClient<IEmbeddingService, OpenAiEmbeddingService>((provider, client) =>
            ConfigureOpenAiClient(provider, client, configuration));
        services.AddHttpClient<IChatCompletionService, OpenAiChatCompletionService>((provider, client) =>
            ConfigureOpenAiClient(provider, client, configuration));

        return services;
    }

    private static void ConfigureOpenAiClient(
        IServiceProvider provider,
        HttpClient client,
        IConfiguration configuration)
    {
        var baseUrl = configuration[$"{OpenAiOptions.SectionName}:BaseUrl"]
            ?? "https://api.openai.com/v1";
        client.BaseAddress = new Uri($"{baseUrl.TrimEnd('/')}/");
        client.Timeout = TimeSpan.FromSeconds(120);
    }

    /// <summary>
    /// Development-only: apply pending migrations, then run the idempotent seeder.
    /// Called by Portfolio.Api when the environment is Development.
    /// </summary>
    public static Task MigrateAndSeedAsync(this IServiceProvider serviceProvider)
        => MigrateAndSeedAsync(serviceProvider, seed: true);

    /// <summary>
    /// Production-safe startup helper: migrate and optionally seed based on
    /// <c>Database:MigrateOnStartup</c> / <c>Database:SeedOnStartup</c> flags.
    /// Prefer running <c>dotnet ef database update</c> from CI for production;
    /// this path exists for simple single-instance hosts (Render/Fly/Azure).
    /// </summary>
    public static async Task MigrateAndSeedAsync(this IServiceProvider serviceProvider, bool seed)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();

        await db.Database.MigrateAsync();
        if (seed)
        {
            await SeedData.RunAsync(db);
        }
    }

    /// <summary>Migrate only (no seeding). Used when seed content is managed via Admin CMS.</summary>
    public static async Task MigrateOnlyAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();

        await db.Database.MigrateAsync();
    }
}
