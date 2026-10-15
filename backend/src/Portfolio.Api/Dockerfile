# .NET 9 — multi-stage production image for Portfolio.Api.
# Build context: repository root (so the solution + all projects are visible).
#   docker build -f backend/src/Portfolio.Api/Dockerfile -t portfolio-api .
# Run (production):
#   docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production \
#     -e ConnectionStrings__PortfolioDb="Host=...;..." \
#     -e Cors__AllowedOrigins__0="https://www.example.com" \
#     -e Ai__OpenAi__ApiKey="sk-..." \
#     portfolio-api
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first (better layer caching): copy solution + project files only.
COPY global.json ./
COPY backend/Portfolio.sln ./backend/
COPY backend/Directory.Build.props ./backend/
COPY backend/NuGet.config ./backend/
COPY backend/src/Portfolio.Api/Portfolio.Api.csproj ./backend/src/Portfolio.Api/
COPY backend/src/Portfolio.Application/Portfolio.Application.csproj ./backend/src/Portfolio.Application/
COPY backend/src/Portfolio.Domain/Portfolio.Domain.csproj ./backend/src/Portfolio.Domain/
COPY backend/src/Portfolio.Infrastructure/Portfolio.Infrastructure.csproj ./backend/src/Portfolio.Infrastructure/
RUN dotnet restore backend/Portfolio.sln

# Full source + publish (Release, single framework, no app host bloat).
COPY backend/ ./backend/
WORKDIR /src/backend
RUN dotnet publish src/Portfolio.Api/Portfolio.Api.csproj \
    -c Release -o /app/publish --no-restore \
    -p:GenerateDocumentationFile=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# curl for the container healthcheck (platforms also probe /health over HTTP).
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

# Non-root user (defense in depth; Kestrel on 8080 needs no privileges).
USER app

# Liveness probe matches the /health endpoint wired in Program.cs.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]
