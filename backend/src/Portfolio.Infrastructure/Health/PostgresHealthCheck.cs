using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Portfolio.Infrastructure.Health;

/// <summary>
/// PostgreSQL reachability check using the existing Npgsql dependency
/// (no extra health-check package). Opens and disposes a connection;
/// never exposes connection strings or internal details — the failure
/// message is generic, full exception goes to logs only.
/// </summary>
public sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresHealthCheck> _logger;

    public PostgresHealthCheck(IConfiguration configuration, ILogger<PostgresHealthCheck> logger)
    {
        _connectionString = configuration.GetConnectionString(DependencyInjection.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{DependencyInjection.ConnectionStringName}' is not configured.");
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1;", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL is reachable.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "PostgreSQL health check failed.");
            return HealthCheckResult.Unhealthy("PostgreSQL is unreachable.");
        }
    }
}
