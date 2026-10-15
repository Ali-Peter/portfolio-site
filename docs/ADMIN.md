# Admin CMS — setup & operations

The Admin CMS gives a single administrator JWT-authenticated CRUD access to
Projects, Blog, Skills and Experience, plus the contact inbox and the RAG
search reindex. Public APIs are unchanged — the admin surface lives entirely
under `api/v1/admin/*` and is ignored by anonymous visitors.

## Routes

| Area | Endpoint(s) |
|---|---|
| Login (rate-limited: 10/min/IP) | `POST /api/v1/admin/auth/login` |
| Projects | `GET/POST /api/v1/admin/projects`, `GET/PUT/DELETE /api/v1/admin/projects/{id}` |
| Blog | `GET/POST /api/v1/admin/blog`, `GET/PUT/DELETE /api/v1/admin/blog/{id}` |
| Skills | `GET/POST /api/v1/admin/skills`, `PUT/DELETE /api/v1/admin/skills/{id}` |
| Experience | `GET/POST /api/v1/admin/experience`, `PUT/DELETE /api/v1/admin/experience/{id}` |
| Inbox | `GET /api/v1/admin/inbox`, `PATCH /api/v1/admin/inbox/{id}/handled`, `DELETE /api/v1/admin/inbox/{id}` |
| Reindex | `POST /api/v1/admin/reindex` (synchronous, returns chunk count) |

Every endpoint except `login` requires `Authorization: Bearer <token>`.
Mutations queue a **fire-and-forget reindex** in a background scope; the
explicit `/admin/reindex` endpoint runs synchronously for manual recovery.

## Configuration

| Setting | Required | Notes |
|---|---|---|
| `Jwt:Key` | **yes** | Signing key, **≥ 32 characters** (256-bit). The API exits at startup if missing/short. |
| `Jwt:Issuer` | no | Default `portfolio-api` |
| `Jwt:Audience` | no | Default `portfolio-admin` |
| `Jwt:ExpiryMinutes` | no | Default `720` (12 h) |
| `Admin:Email` | **yes** | Login identity (case-insensitive). Startup warning when empty. |
| `Admin:PasswordHash` | prod **yes** | PBKDF2 hash, format `v1.{iterations}.{salt}.{hash}`. Startup warning when empty. |
| `Admin:Password` | dev only | Plaintext fallback — hashed in-memory for comparison, always logs a warning. Never set in production. |

### Generating the password hash

```bash
dotnet run --project backend/src/Portfolio.Api -- --hash-password 'your-strong-password'
```

The command prints one `v1.…` hash line and exits. Put the value (not the
password) into `Admin:PasswordHash`. CLI arguments can appear in shell history
and process listings — run this from a trusted machine.

### Local development (user-secrets)

```bash
cd backend/src/Portfolio.Api
dotnet user-secrets set "Jwt:Key" "<any 32+ char random string>"
dotnet user-secrets set "Admin:Email" "me@example.com"

# Step 1: print a hash for your password…
dotnet run -- --hash-password 'dev-password'
# Step 2: …then store the printed v1.… line (not the password):
dotnet user-secrets set "Admin:PasswordHash" "<paste the v1.… hash here>"
```

Shortcut while developing: `dotnet user-secrets set "Admin:Password" "dev-password"`
(the plaintext escape hatch — Development only, a warning is logged at startup).

### Production (environment variables)

```bash
Jwt__Key=...            # openssl rand -base64 48
Jwt__ExpiryMinutes=720
Admin__Email=admin@example.com
Admin__PasswordHash=v1.210000....
```

Same names as in `.env.production.example` / `docs/DEPLOY.md`.

## Using the admin UI

- The frontend area is `/admin` (redirects to `/admin/login` without a token).
- The bearer token lives in memory + `localStorage` (`portfolio.admin.*`);
  a page refresh keeps the session, an expired/revoked token (401) bounces
  back to the login page and clears it.
- Login attempts are rate-limited per IP (10/min); production sits behind HTTPS.

## Security rules (do not break these)

- No credentials in the repo — configuration only (user-secrets / env vars).
- Never log tokens, passwords or hashes.
- Keep `Admin:Password` unset outside Development.
- Rotate `Jwt:Key` to invalidate all sessions (tokens are stateless).
