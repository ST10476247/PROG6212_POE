# ============================================================
# RaceDay.API — Multi-stage Dockerfile (ASP.NET Core Web API)
# ============================================================

# ── Stage 1: base runtime image ──────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# ── Stage 2: SDK build image ──────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project file first (better layer caching)
COPY ["RaceDay.API/RaceDay.API.csproj", "RaceDay.API/"]

# Restore packages
RUN dotnet restore "RaceDay.API/RaceDay.API.csproj"

# Copy the rest of the source
COPY . .

WORKDIR "/src/RaceDay.API"

# Build
RUN dotnet build "RaceDay.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ── Stage 3: publish ──────────────────────────────────────────
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RaceDay.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# ── Stage 4: final runtime image ─────────────────────────────
FROM base AS final
WORKDIR /app

# Create uploads directory for local blob fallback
RUN mkdir -p wwwroot/uploads

# Copy published output from publish stage
COPY --from=publish /app/publish .

# Run as non-root for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "RaceDay.API.dll"]

