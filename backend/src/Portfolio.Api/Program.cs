using System.Globalization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api;
using Portfolio.Api.Middleware;
using Portfolio.Api.Security;
using Portfolio.Application;
using Portfolio.Application.Common;
using Portfolio.Application.Services;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Health;

// Admin credential helper: prints the PBKDF2 hash for the `Admin:PasswordHash`
// setting (see docs/ADMIN.md).
//   dotnet run --project src/Portfolio.Api -- --hash-password 'your-password'
// Note: CLI args can leak via shell history / process listings — use a trusted
// machine and prefer interactive entry for production credentials.
if (args is ["--hash-password", { } password])
{
    Console.WriteLine(AdminPasswordHasher.Hash(password));
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Reverse-proxy support: configure before Build so UseForwardedHeaders has
// options. Trust the platform proxy (Render/Fly/Azure terminate TLS at the
// edge and forward X-Forwarded-Proto/For).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Cloud hosts sit behind proxies with unknown IPs; clearing the allow-list
    // is the documented way to honor X-Forwarded-* on Render/Fly/Azure.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Layers are wired in exactly one place (the composition root).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// RAG assistant tuning (TopK, input limits). The OpenAI section itself is
// bound in Infrastructure; its API key only ever comes from user-secrets or
// environment variables — never the repository.
builder.Services.Configure<AiAssistantOptions>(
    builder.Configuration.GetSection(AiAssistantOptions.SectionName));

// Controllers + API versioning (URL segment, default v1 for unversioned routes).
builder.Services
    .AddControllers()
    // Enums serialize as their string names (e.g. "Completed", "Expert"),
    // matching the Angular string-union models. Numeric enums would leak raw
    // ordinals to the client and break the typed DTO contract.
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Invalid request bodies also surface as RFC 9457 with an errors extension,
        // consistent with the global exception middleware (422, not the 400 default).
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(m => m.ErrorMessage).ToArray(),
                    StringComparer.Ordinal);

            return new UnprocessableEntityObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation failed",
                Detail = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = context.HttpContext.TraceIdentifier,
                    ["errors"] = errors,
                },
            });
        };
    });
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// Explicit CORS: origins come from configuration, never AllowAnyOrigin.
// Production appsettings.json ships an empty list (fail-closed); the deploy
// MUST set Cors:AllowedOrigins (or Cors__AllowedOrigins env) to the live
// frontend URL(s), e.g. https://www.example.com.
var corsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "portfolio",
        policy =>
        {
            if (corsOrigins.Length == 0)
            {
                // Fail closed: no origins allowed until the deploy sets them.
                // WithOrigins() with an empty array would throw at startup,
                // so short-circuit to a policy that allows nothing.
                policy.WithMethods("GET", "POST", "OPTIONS");
                return;
            }

            policy
                .WithOrigins(corsOrigins)
                .WithMethods("GET", "POST", "OPTIONS")
                .WithHeaders("Content-Type", "Authorization")
                .SetPreflightMaxAge(TimeSpan.FromHours(1));
        });
});

// Rate limiting: a generous global fixed window plus a strict per-IP policy
// for the contact endpoint. Rejections return RFC 9457 ProblemDetails (429).
var globalLimit = builder.Configuration.GetValue("RateLimiting:Global:PermitLimit", 600);
var globalWindow = builder.Configuration.GetValue("RateLimiting:Global:WindowSeconds", 60);
var contactLimit = builder.Configuration.GetValue("RateLimiting:Contact:PermitLimit", 5);
var contactWindow = builder.Configuration.GetValue("RateLimiting:Contact:WindowSeconds", 60);
var aiLimit = builder.Configuration.GetValue("RateLimiting:Ai:PermitLimit", 10);
var aiWindow = builder.Configuration.GetValue("RateLimiting:Ai:WindowSeconds", 60);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        var http = context.HttpContext;
        http.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        // Fixed-window rejections carry Metadata with RetryAfter; surface it
        // as a header when present so well-behaved clients back off correctly.
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            http.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(
                CultureInfo.InvariantCulture);
        }

        await http.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = "Rate limit exceeded. Please try again later.",
                Instance = http.Request.Path,
                Extensions = { ["traceId"] = http.TraceIdentifier },
            },
            cancellationToken: cancellationToken);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = globalLimit,
                Window = TimeSpan.FromSeconds(globalWindow),
                QueueLimit = 0,
            }));

    options.AddPolicy(
        RateLimitingPolicies.Contact,
        context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = contactLimit,
                Window = TimeSpan.FromSeconds(contactWindow),
                QueueLimit = 0,
            }));

    // The assistant is the only endpoint that costs real money per request
    // (embeddings + completion), so it gets the strictest policy.
    options.AddPolicy(
        RateLimitingPolicies.Ai,
        context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = aiLimit,
                Window = TimeSpan.FromSeconds(aiWindow),
                QueueLimit = 0,
            }));

    // Admin login: strict per-IP policy against credential stuffing.
    options.AddPolicy(
        RateLimitingPolicies.AdminAuth,
        context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(60),
                QueueLimit = 0,
            }));
});

// Health: /health is liveness (always 200 when the process runs);
// /health/ready includes the PostgreSQL reachability check.
builder.Services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgres");

// Admin CMS (v1): single-admin JWT auth. Credentials and the signing key come
// from configuration only (user-secrets locally, env vars in production) —
// never the repository. A missing/short key fails fast at startup rather than
// silently issuing weak tokens.
builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();

var jwtKey = builder.Configuration.GetValue<string>("Jwt:Key") ?? string.Empty;
if (jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT signing key is not configured. Set it via user-secrets (Jwt:Key, min 32 chars) " +
        "locally or the Jwt__Key environment variable in production.");
}

builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetValue("Jwt:Issuer", "portfolio-api"),
            ValidAudience = builder.Configuration.GetValue("Jwt:Audience", "portfolio-admin"),
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2),
        };

        // Admin routes are Authorization-header bearer tokens; failures must be
        // RFC 9457 JSON (401/403), not the default empty challenge/redirect.
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                return context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "Authentication is required to access this resource.",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = context.HttpContext.TraceIdentifier },
                });
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";
                return context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Detail = "You do not have permission to access this resource.",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = context.HttpContext.TraceIdentifier },
                });
            },
        };
    });
builder.Services.AddAuthorization();

// Swagger only in Development (never exposed in production).
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new() { Title = "Portfolio API", Version = "v1" });

        // Admin CMS (v1): authorize button sends the login bearer token.
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "Admin JWT from POST /api/v1/admin/auth/login. Enter: Bearer {token}",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
        });
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                []
            },
        });

        var xml = Path.Combine(AppContext.BaseDirectory, "Portfolio.Api.xml");
        if (File.Exists(xml))
        {
            options.IncludeXmlComments(xml, includeControllerXmlComments: true);
        }
    });
}

var app = builder.Build();

// Reverse-proxy headers (Render / Fly / Azure / Nginx): honor X-Forwarded-*
// so HTTPS redirects, HSTS and rate-limit IPs work behind the platform proxy.
app.UseForwardedHeaders();

// Exceptions first so every failure below renders as ProblemDetails.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    // HSTS + HTTPS redirect are platform-managed on most PaaS hosts; HSTS is
    // safe to keep here because production always serves HTTPS at the edge.
    app.UseHsts();
}

app.UseCors("portfolio");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    // Warn when admin auth falls back to plaintext (no hash configured).
    var adminHash = app.Configuration.GetValue<string>("Admin:PasswordHash") ?? string.Empty;
    var adminPassword = app.Configuration.GetValue<string>("Admin:Password") ?? string.Empty;
    var adminEmail = app.Configuration.GetValue<string>("Admin:Email") ?? string.Empty;
    if (string.IsNullOrWhiteSpace(adminEmail))
    {
        app.Logger.LogWarning("Admin:Email is not configured — admin login will reject every attempt.");
    }

    if (string.IsNullOrWhiteSpace(adminHash))
    {
        app.Logger.LogWarning(
            "Admin:PasswordHash is not configured — falling back to Admin:Password plaintext comparison. " +
            "Development only: generate a hash for production (see docs/ADMIN.md).");
    }

    if (!string.IsNullOrWhiteSpace(adminPassword) && !app.Environment.IsDevelopment())
    {
        app.Logger.LogWarning("Admin:Password plaintext is set in a non-Development environment — prefer Admin:PasswordHash.");
    }

    // First run: apply pending migrations, then seed real content (idempotent).
    await app.Services.MigrateAndSeedAsync();

    // Rebuild the RAG index in the background once the schema is current.
    // Skips itself (with a log line) when no OpenAI key is configured.
    _ = Task.Run(async () =>
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var indexer = scope.ServiceProvider.GetRequiredService<IPortfolioIndexer>();
            await indexer.ReindexAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Background AI indexing failed.");
        }
    });
}
else
{
    // Production startup is opt-in via configuration (env vars on the host):
    //   Database__MigrateOnStartup=true  -> run EF migrations (simple hosts)
    //   Database__SeedOnStartup=true     -> also run the idempotent seeder
    // Prefer `dotnet ef database update` from CI for zero-downtime deploys;
    // this path exists so single-instance hosts (Render/Fly/Azure) just work.
    var migrateOnStartup = app.Configuration.GetValue("Database:MigrateOnStartup", false);
    var seedOnStartup = app.Configuration.GetValue("Database:SeedOnStartup", false);
    if (migrateOnStartup)
    {
        app.Logger.LogInformation("Applying production database migrations on startup.");
        if (seedOnStartup)
        {
            await app.Services.MigrateAndSeedAsync(seed: true);
        }
        else
        {
            await app.Services.MigrateOnlyAsync();
        }
    }
}

app.Run();

// Partial class so WebApplicationFactory-based integration tests can boot the API.
public partial class Program;
