# GEMINI.md - CoreBackendApp Development Guidelines

## Project Context
CoreBackendApp is a modular monolith built on .NET 10, designed to be a central SaaS platform for multiple construction and project management applications (AxiomWork, StructinoTrust).

## Tech Stack
- **Framework:** .NET 10
- **API Style:** Minimal APIs (with some existing Controllers)
- **Persistence:** Entity Framework Core (SQL Server)
- **Auth:** JWT-based, Policy-based authorization (Roles, Permissions, Features)
- **Data Transfer:** Immutable C# records for DTOs

## Architecture (Clean Architecture)
- **CoreBackendApp.Api:** HTTP endpoints, authentication/authorization configuration, middleware.
- **CoreBackendApp.Application:** Business logic, service interfaces, DTOs (using records).
- **CoreBackendApp.Domain:** Core entities, interfaces, and domain rules.
- **CoreBackendApp.Infrastructure:** DB context, EF Core configurations, repositories, and migrations.

## Development Mandates
1. **Clean Architecture:** Maintain strict separation of concerns between layers. Do not reference `Infrastructure` or `Api` from `Domain` or `Application`.
2. **DTOs:** Always use `public record` for Request and Response models in the `Application` layer.
3. **Minimal APIs:** Prefer Minimal APIs for new endpoints, organized into separate classes/files (e.g., in `Endpoints/` folder).
4. **Authentication:** Ensure all new sensitive endpoints are protected by appropriate granular permissions (e.g., `.RequireAuthorization("user.manage")`). Use `HasPermissionAttribute` for controllers if applicable.
5. **EF Core:** Use Migrations for any database changes. Ensure entities inherit from `BaseEntity` where applicable.
6. **Code Style:** Follow standard C# and .NET 10 conventions. Use implicit usings and file-scoped namespaces.

## Key Files & Directories
- `CoreBackendApp.Infrastructure/Persistence/CoreDbContext.cs`: Main DB Context.
- `CoreBackendApp.Application/Auth/AuthService.cs`: Logic for authentication.
- `CoreBackendApp.Api/Endpoints/`: Location for Minimal API definitions.
- `CoreBackendApp.Domain/Entities/`: Domain models.
