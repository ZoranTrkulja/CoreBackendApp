# GEMINI.md - CoreBackendApp Development Guidelines

## Project Context
CoreBackendApp is a modular monolith built on .NET 10, serving as a SaaS backend for construction/project management systems.

---

## Tech Stack
- .NET 10
- Minimal APIs (primary)
- Entity Framework Core (SQL Server)
- JWT Authentication + Policy-based Authorization
- DTOs: immutable C# records

---

## Architecture Rules (Clean Architecture)

### Layer Responsibilities
- Api → HTTP handling only (NO business logic)
- Application → business logic, DTOs, interfaces
- Domain → entities, core rules
- Infrastructure → EF Core, DB, repositories

### Strict Dependency Rule
- Domain → no dependencies
- Application → depends only on Domain
- Infrastructure → depends on Application + Domain
- Api → depends on Application

❌ NEVER:
- Reference Infrastructure from Application/Domain
- Put business logic in Api
- Expose EF entities outside Infrastructure

---

## Strict Rules (DO / DON'T)

### DO
- Use Minimal APIs for all new endpoints
- Use `record` for all DTOs
- Use async/await everywhere
- Return `IResult` from endpoints
- Use dependency injection via constructor
- Use feature-based folder structure

### DON'T
- Do NOT use Controllers for new features
- Do NOT expose EF entities directly
- Do NOT inject DbContext into Api layer
- Do NOT write sync code in API
- Do NOT skip authorization on endpoints

---

## Endpoint Pattern (MANDATORY)

```csharp
app.MapPost("/users", async (CreateUserRequest request, IUserService service) =>
{
    var result = await service.CreateUserAsync(request);
    return Results.Ok(result);
})
.RequireAuthorization("user.create");