# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copy the solution file from the subfolder
COPY CoreBackendApp/CoreBackendApp.sln ./

# 2. Copy csproj files from their respective subfolders
COPY CoreBackendApp/CoreBackendApp.Api/*.csproj CoreBackendApp.Api/
COPY CoreBackendApp/CoreBackendApp.Application/*.csproj CoreBackendApp.Application/
COPY CoreBackendApp/CoreBackendApp.Infrastructure/*.csproj CoreBackendApp.Infrastructure/
COPY CoreBackendApp/CoreBackendApp.Domain/*.csproj CoreBackendApp.Domain/

# 3. Restore
RUN dotnet restore CoreBackendApp.sln

# 4. Copy everything from the inner CoreBackendApp folder to /src
COPY CoreBackendApp/ .

# 5. Publish
RUN dotnet publish CoreBackendApp.Api/CoreBackendApp.Api.csproj \
    -c Release \
    -o /app/publish

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CoreBackendApp.Api.dll"]