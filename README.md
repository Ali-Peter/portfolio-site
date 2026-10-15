# Portfolio — Ali Ogochukwu Peter

Professional portfolio for a **Software Engineer / AI Engineer** position.

## Stack

| Layer    | Technology                                              |
| -------- | ------------------------------------------------------- |
| Backend  | ASP.NET Core 9 · Clean Architecture (4 projects)        |
| Frontend | Angular 20 (standalone, zoneless, SSR) · Tailwind CSS 4 |
| Database | PostgreSQL 16 + pgvector                                |
| Errors   | RFC 9457 `ProblemDetails` for all API errors            |

## Repository layout

```
portfolio-site/
├── backend/                  # .NET solution
│   ├── src/
│   │   ├── Portfolio.Api/             # Composition root: Program.cs, health, controllers (Phase 2)
│   │   ├── Portfolio.Application/     # Use-case layer: services, DTOs, validators (Phase 2)
│   │   ├── Portfolio.Domain/          # Entities & domain rules — zero dependencies
│   │   └── Portfolio.Infrastructure/  # EF Core + Npgsql, DbContext, migrations, seed
│   └── tests/
│       ├── Portfolio.UnitTests/
│       ├── Portfolio.IntegrationTests/
│       └── Portfolio.ArchitectureTests/
├── frontend/                 # Angular 20 workspace
├── docs/adr/                 # Architecture Decision Records
├── docker-compose.yml        # PostgreSQL 16 + pgvector (local dev)
├── .env.example              # documented local-dev variables (no real secrets)
└── global.json               # pinned .NET SDK
```

**Dependency rule (enforced by tests):** `Api → Application → Domain ← Infrastructure`.
`Domain` references nothing.

## Quick start

### 1. Database

```bash
cp .env.example .env        # optional — defaults work out of the box
docker compose up -d
```

### 2. Backend

```bash
cd backend
dotnet tool restore          # local dotnet-ef
dotnet build                 # 0 warnings, 0 errors (warnings = errors)
dotnet run --project src/Portfolio.Api
```

In the `Development` environment the API **migrates and seeds automatically** on start.
Verify: `http://localhost:5000/health` (liveness), `http://localhost:5000/health/ready` (incl. PostgreSQL).

Swagger (Development only): `http://localhost:5000/swagger`.

## API (v1)

Base path: `/api/v1/`. All errors are RFC 9457 `ProblemDetails`; paging
validation failures return `422`, unknown slugs return `404`.

| Method | Route | Notes |
| ------ | ----- | ----- |
| GET | `/projects?page=1&pageSize=20&tag=` | Paged list, optional exact technology tag |
| GET | `/projects/{slug}` | Single project, `404` when unknown |
| GET | `/skills` | All skills, enum-category order then sort order |
| GET | `/experience` | All experience entries in display order |
| GET | `/blog?page=1&pageSize=20&search=` | Published posts only, newest first |
| GET | `/blog/{slug}` | Single published post, `404` when unknown |
| POST | `/contact` | `201` on accept; `422` on validation failure; `429` after 5/min per IP. A filled `website` honeypot is silently accepted (`201`) without being stored |

Rate limiting: 600 req/min global per IP; the contact policy (5/min per IP)
adds a `Retry-After` header on `429` rejections.
CORS: origins come from `Cors:AllowedOrigins` (Development defaults to
`http://localhost:4200` and `http://localhost:4201`); production
`appsettings.json` deliberately ships an empty list — the deployment must set it.

Manual alternative:

```bash
dotnet ef database update --project src/Portfolio.Infrastructure --startup-project src/Portfolio.Infrastructure
```

### 3. Frontend

```bash
cd frontend
npm install
npm start                    # http://localhost:4200
```

## Configuration & secrets

- **No secrets in the repository.** Local development uses the compose defaults documented in `.env.example`.
- Production configuration is supplied via environment variables
  (`ConnectionStrings__PortfolioDb`, etc.).

## Status

Phase 0 + Phase 1: solution skeleton, domain model, DbContext, first migration, seed data, Angular shell.
Phase 2: application services + validators + AutoMapper, public `/api/v1/` controllers,
global ProblemDetails error handling, rate limiting, explicit CORS,
Development-only Swagger, custom PostgreSQL health check.
Build: 0 warnings, 0 errors. Tests: 39/39 green (26 unit, 11 Testcontainers-Postgres integration, 2 architecture).

See `docs/adr/` for architecture decisions (incl. ADR-0002: mapping with AutoMapper).
