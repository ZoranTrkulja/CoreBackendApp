
# CoreBackendApp – Architecture & Authentication Documentation

## 1. Overview
CoreBackendApp is a central backend platform designed to serve multiple applications:
- AxiomWork – task & project management
- StructinoTrust – investor & construction rating
- Future applications via feature-based extension

The system is built as a **modular monolith**, designed for future microservice extraction.

---

## 2. Technology Stack
- .NET 10
- Minimal APIs
- Entity Framework Core
- JWT Authentication
- Role / Permission / Feature-based Authorization
- Swagger (Swashbuckle 6.6.2)
- BCrypt password hashing

---

## 3. Solution Structure (Clean Architecture)

```
CoreBackendApp
├── CoreBackendApp.Api
├── CoreBackendApp.Application
├── CoreBackendApp.Infrastructure
└── CoreBackendApp.Domain
```

Responsibilities:
- API: HTTP endpoints, auth wiring
- Application: business logic
- Infrastructure: DB, EF Core, repositories
- Domain: entities and core rules

---

## 4. Authentication (JWT)

Login flow:
1. Client calls /auth/login
2. Backend validates credentials (BCrypt)
3. Loads roles, permissions, features
4. Issues JWT token

JWT Claims:
- sub (UserId)
- tenant_id
- roles
- permissions
- features

JWT Configuration:
- HMAC SHA256
- Minimum 32-byte secret key
- Short-lived access token (15 min)

---

## 5. Authorization

Policy-based authorization:
- Role-based
- Permission-based
- Feature-based

Example policy:
```
RequireUsersManagePermission
```

Used in Minimal API:
```
.RequireAuthorization("RequireUsersManagePermission")
```

---

## 6. Swagger Integration

- Swagger configured with JWT Bearer authentication
- Authorize button enabled
- Supports secured endpoint testing

---

## 7. Entity Framework Core

- SQL Server
- Code-first migrations
- Automatic database seeding:
  - Admin tenant
  - Admin user
  - Default roles, permissions, features

---

## 8. DTO Strategy

Uses immutable C# records for request/response models:
```
public record LoginRequest(string Email, string Password);
```

---

## 9. Token Usage

- Backend generates & validates JWT
- Client stores token and sends it via:
```
Authorization: Bearer <token>
```

---

## 10. Repository Setup

- Private repository
- License: NONE (All Rights Reserved)
- bin/ and obj/ excluded via .gitignore

---

## 11. Current Status

✔ Authentication working  
✔ Authorization policies active  
✔ Swagger secured  
✔ EF Core migrations & seed complete  

---

## 12. Next Steps

- User Management API
- Refresh Token system
- AxiomWork core modules
- StructinoTrust core modules

---

## 13. Architectural Vision

CoreBackendApp is designed as a reusable SaaS core platform with:
- Multi-tenancy
- Feature flags
- White-label readiness
