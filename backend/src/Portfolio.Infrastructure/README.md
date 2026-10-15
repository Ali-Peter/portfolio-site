# Portfolio.Infrastructure

EF Core (Npgsql) implementation: `PortfolioDbContext`, entity configurations,
design-time migration factory, idempotent `SeedData`, and DI registration
(`AddInfrastructure` / `MigrateAndSeedAsync`).

References: `Portfolio.Application` (+ `Domain` transitively).
