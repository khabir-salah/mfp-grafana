# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY src/MfpDashboard/MfpDashboard.csproj ./MfpDashboard/
RUN dotnet restore ./MfpDashboard/MfpDashboard.csproj

COPY src/MfpDashboard/ ./MfpDashboard/

RUN dotnet publish ./MfpDashboard/MfpDashboard.csproj \
    -c Release \
    -o /app/publish

# ── Runtime stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app/uploads /app/logs

COPY --from=build-env /app/publish .

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/api/upload/status || exit 1

ENTRYPOINT ["dotnet", "MfpDashboard.dll"]