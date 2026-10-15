# Production deployment — public portfolio site + Admin CMS

Goal: public site live end-to-end (API + Angular SSR + Postgres/pgvector + RAG
chatbot) plus the `/admin` CMS. Admin-specific setup (JWT key, admin
credentials, hash generation) is documented in `docs/ADMIN.md`.

## 1. Production configuration (env vars only — zero secrets in the repo)

The API reads everything from environment variables in Production.
`appsettings.json` ships safe non-secret defaults; `Cors.AllowedOrigins`
is intentionally empty (fail-closed); `Ai.OpenAi.ApiKey` is empty (chatbot
reports "not configured" until a key is set). No `appsettings.Production.json`
is committed — the host's env vars are the production config.

| Env var | Required | Example |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | yes | `Production` |
| `ConnectionStrings__PortfolioDb` | yes | `Host=db.xxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=***;SSL Mode=Require;Trust Server Certificate=true` |
| `Cors__AllowedOrigins__0` | yes | `https://www.example.com` |
| `Ai__OpenAi__ApiKey` | chatbot only | `sk-...` (host secret; empty = chatbot 503 mode, rest of site works) |
| `Ai__OpenAi__ChatModel` | no | `gpt-4o-mini` |
| `Ai__OpenAi__EmbeddingModel` | no | `text-embedding-3-small` (must stay 1536-dim, matches `vector(1536)` migration) |
| `Ai__TopK` / `Ai__MaxQuestionChars` / `Ai__MaxContextChars` | no | `6` / `2000` / `9000` |
| `Database__MigrateOnStartup` | simple hosts | `true` = run EF migrations at boot (Render/Fly/compose). Prefer CI `dotnet ef` for fleets |
| `Database__SeedOnStartup` | first deploy | `true` = run idempotent seeder (skips when data exists). `false` once Admin CMS owns content |
| `Jwt__Key` | yes (admin) | JWT signing key, ≥ 32 chars (`openssl rand -base64 48`). API **exits at startup** without it |
| `Jwt__Issuer` / `Jwt__Audience` | no | defaults `portfolio-api` / `portfolio-admin` |
| `Jwt__ExpiryMinutes` | no | default `720` (12 h) |
| `Admin__Email` | yes (admin) | admin login identity (startup warning when empty) |
| `Admin__PasswordHash` | yes (prod) | PBKDF2 `v1.…` hash — generate with `dotnet run -- --hash-password '…'` (see `docs/ADMIN.md`) |

CORS origins must be exact `https` URLs with no trailing slash. Add `__1`,
`__2`… for apex + www + preview URLs.

Frontend: `src/environments/environment.prod.ts` holds the `__API_BASE_URL__`
placeholder. Set it at build time with the Docker build arg:

```bash
docker build -f Dockerfile --build-arg API_BASE_URL=https://api.example.com -t portfolio-web ./frontend
```

## 2. Database

- Provider: any managed Postgres 16 with the pgvector extension (Supabase,
  Neon with `vector` enabled, Render, Azure Flexible Server, or self-hosted
  `pgvector/pgvector:pg16`).
- Extension: the `AddContentChunks` migration runs
  `CREATE EXTENSION IF NOT EXISTS vector;`. Where the app role cannot create
  extensions, enable `vector` once in the provider dashboard/SQL editor first.
- Migrations (two paths):
  - A. CI/CD (recommended): run once per deploy with the prod connection string:
    `dotnet ef database update --project src/Portfolio.Infrastructure --startup-project src/Portfolio.Api`.
    Keep `Database__MigrateOnStartup=false` so instances never race.
  - B. Migrate on startup (simple hosts): set `Database__MigrateOnStartup=true`.
- Seeding: first deploy `Database__SeedOnStartup=true` (idempotent — returns
  early when Projects exist). After that, or once Admin CMS owns content,
  set `false`. Seed content = real portfolio rows in `SeedData.*.cs`; the RAG
  `content_chunks` index is built from those rows (see section 5).

## 3. Recommended hosting (simple + low cost)

API and SSR frontend ship as separate containers (Node vs .NET runtimes;
hosts bill/scale them independently). Postgres stays managed.

### Option A — Render + Supabase (simplest)

API: Render Web Service (Docker, `backend/src/Portfolio.Api/Dockerfile`,
context = repo root, health check `/health`, port 8080).
Frontend: Render Web Service (Docker, `frontend/Dockerfile`,
context = `frontend/`, build arg `API_BASE_URL`).
Postgres: Supabase free tier (Postgres 16 + pgvector; enable `vector`).

Steps: Supabase project + Npgsql string; API service with prod connection
string, OpenAI key, `Database__MigrateOnStartup=true`,
`Database__SeedOnStartup=true` (first deploy); frontend service with the API
URL as build arg; copy the frontend URL back into the API's
`Cors__AllowedOrigins__0` and redeploy; verify (section 5); flip seed to false.

### Option B — Single VPS with compose (cheapest long-term)

```bash
cp .env.production.example .env.production   # fill in real values
docker compose -f docker-compose.production.yml --env-file .env.production up -d --build
```

Front with Caddy/Nginx for automatic HTTPS. With managed Postgres run only
`api` + `web`. First boot with migrate/seed `true`, then set both `false`.

## 4. Docker

- `backend/src/Portfolio.Api/Dockerfile` (.NET 9 multi-stage, non-root,
  `HEALTHCHECK /health`, port 8080, context = repo root).
- `frontend/Dockerfile` (Node 22 multi-stage, `API_BASE_URL` build arg,
  SSR on `PORT`, default 4000).
- `docker-compose.yml` (local dev Postgres, unchanged).
- `docker-compose.production.yml` (prod `db` + `api` + `web`).

## 5. Post-deploy checklist & verification

Config: prod connection string with SSL; CORS origin = exact frontend URL;
OpenAI key in host secrets (or deliberately empty); frontend built with the
real API URL; seed flags true/true first, false/false after; secret scan clean.

Health: `GET /health` 200; `GET /health/ready` 200; `GET /swagger` 404;
HSTS present; evil-origin CORS probe returns no allow-origin header.

```bash
API=https://<api-host>
curl -s $API/api/v1/projects | head -c 300
curl -s -X POST $API/api/v1/contact -H 'Content-Type: application/json' \
  -d '{"name":"Deploy Check","email":"deploy@example.com","message":"Hello from production verification."}'
```

Pages `/`, `/projects`, `/blog`, `/contact`, `/ai-assistant` render; contact
submits live; chatbot answers after the one-off RAG reindex (resolve
`IPortfolioIndexer`, call `ReindexAsync()` with prod string + key), else it
explains it is unconfigured. Production boot never auto-reindexes because
embeddings cost per run — repeat after content changes until Admin CMS
automates it.

