# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Restore first, copying only project files, so this layer is cached
# until a .csproj changes (not on every source edit).
COPY nuget.config ./
COPY src/Domain/CineVault.Domain.csproj src/Domain/
COPY src/Application/CineVault.Application.csproj src/Application/
COPY src/Infrastructure/CineVault.Infrastructure.csproj src/Infrastructure/
COPY src/Api/CineVault.Api.csproj src/Api/
RUN dotnet restore src/Api/CineVault.Api.csproj

COPY src/ src/
RUN dotnet publish src/Api/CineVault.Api.csproj -c Release -o /app --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# SQLite file lives in a mounted volume, writable by the non-root app user.
RUN mkdir /data && chown app:app /data
ENV ConnectionStrings__Default="Data Source=/data/cinevault.db"

COPY --from=build /app .

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "CineVault.Api.dll"]
