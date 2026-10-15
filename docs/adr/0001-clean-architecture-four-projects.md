# 1. Four-project Clean Architecture split

- Date: 2026-10-09
- Status: Accepted

## Context

The portfolio needs to demonstrate professional backend architecture while staying
small enough to maintain solo. The reference production project (`smsbackend`) uses a
7-project layering; a portfolio does not need that much granularity.

## Decision

Split the backend into exactly four projects:

1. `Portfolio.Domain` — entities and domain rules. **References nothing.**
2. `Portfolio.Application` — use-case/service interfaces, DTOs, validators. References `Domain` only.
3. `Portfolio.Infrastructure` — EF Core (Npgsql), DbContext, migrations, seed, external integrations. References `Application` (+ `Domain` transitively).
4. `Portfolio.Api` — composition root: `Program.cs`, controllers, middleware. References `Application` + `Infrastructure`.

Layer dependency rules are enforced by `Portfolio.ArchitectureTests` so they cannot rot.

## Consequences

- Clear, reviewable dependency direction; UI/framework concerns never leak into domain code.
- Migrations live next to the `DbContext` in `Infrastructure`.
- Slight extra ceremony vs. a single project — accepted as the point of the exercise.
