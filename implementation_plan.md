# FocusTrack – Phase 1: Foundation & Auth

## Overview

Bootstrap the **FocusTrack** application from scratch. This phase delivers:
- A **.NET 8 Web API** backend (`FocusTrack.Api`) with EF Core + Oracle provider, JWT Bearer auth, and BCrypt password hashing.
- An **Angular 17+ standalone** frontend (`focus-track-ui`) with login/register components, an `AuthService`, and a JWT HTTP interceptor.

The end result is a fully working register → login → JWT-protected endpoint flow.

---

## User Review Required

> [!IMPORTANT]
> **Oracle EF Core Provider**: The workflow specifies `Oracle.EntityFrameworkCore`. This requires an **Oracle Database** instance (on-prem or OCI). During Phase 1 the connection string will be a placeholder; no migrations or actual DB calls will be attempted. Confirm this is acceptable, or indicate if you want to swap to SQLite/SQL Server for local dev.

> [!IMPORTANT]
> **JWT Secret**: Per security rules, the JWT signing key will be read from environment variables / `appsettings.Development.json` (not hardcoded). You will need to set the `Jwt:Key` environment variable or add it to your local secrets before the API can issue tokens.

> [!WARNING]
> **Angular version**: The workflow uses `--standalone true`. This requires **Angular CLI v17+**. The plan assumes `@angular/cli` v17 or v18 is globally installed. The `ng` commands will be run inside `e:\Learning Projects\AntiGravityTest1\focus-track-ui\`.

---

## Proposed Changes

### Backend – `FocusTrack.Api`

#### [NEW] `FocusTrack.Api/` (dotnet webapi project)
Scaffolded via `dotnet new webapi -n FocusTrack.Api --use-controllers`.

#### [NEW] `FocusTrack.Api/Models/User.cs`
```csharp
public class User {
    Guid Id, string Email, string PasswordHash, DateTime CreatedAt
}
```

#### [NEW] `FocusTrack.Api/Models/Session.cs`
```csharp
public class Session {
    Guid Id, Guid UserId, string AppName, string WindowTitle,
    DateTime StartTime, DateTime EndTime, bool IsBrowser
}
```

#### [NEW] `FocusTrack.Api/Data/FocusDbContext.cs`
EF Core `DbContext` with `DbSet<User>` and `DbSet<Session>`.

#### [MODIFY] `FocusTrack.Api/Program.cs`
- Register `FocusDbContext` with Oracle provider (placeholder connection string from config).
- Configure JWT Bearer middleware.
- Add `AddControllers()` / `MapControllers()`.

#### [NEW] `FocusTrack.Api/Services/AuthService.cs`
- `RegisterAsync(email, password)` — checks for existing user, hashes password with BCrypt, saves to DB.
- `LoginAsync(email, password)` — validates credentials, returns a signed JWT.

#### [NEW] `FocusTrack.Api/Controllers/AuthController.cs`
- `POST /api/auth/register`
- `POST /api/auth/login`

#### [NEW] `FocusTrack.Api/Controllers/TestController.cs`
- `GET /api/test/protected` — decorated with `[Authorize]` for acceptance-criteria validation.

#### [NEW] `FocusTrack.Api/DTOs/` folder
- `RegisterDto.cs` — `Email`, `Password`
- `LoginDto.cs` — `Email`, `Password`
- `AuthResponseDto.cs` — `Token`, `ExpiresAt`

---

### Frontend – `focus-track-ui`

#### [NEW] `focus-track-ui/` (Angular 17+ standalone project)
Scaffolded via `ng new focus-track-ui --standalone true --routing true --style scss`.

#### [NEW] `focus-track-ui/src/app/features/auth/login/`
Login component generated via Angular CLI.

#### [NEW] `focus-track-ui/src/app/features/auth/register/`
Register component generated via Angular CLI.

#### [NEW] `focus-track-ui/src/app/core/auth/auth.service.ts`
Service wrapping `HttpClient` calls to `/api/auth/register` and `/api/auth/login`. Stores JWT in `localStorage`.

#### [MODIFY] `focus-track-ui/src/app/app.routes.ts`
Maps `/login` → `LoginComponent` and `/register` → `RegisterComponent`.

#### [NEW] `focus-track-ui/src/app/core/interceptors/auth.interceptor.ts`
HTTP interceptor that reads the JWT from `localStorage` and attaches it as a `Bearer` token on every outgoing request.

---

## Open Questions

> [!IMPORTANT]
> 1. **Oracle DB**: Do you have an Oracle instance available, or should I configure a **SQLite** fallback for local development so migrations can actually run during Phase 1?
> 2. **Angular CLI**: Do you have `@angular/cli` installed globally (`ng --version`)? If not, I can use `npx @angular/cli` instead.
> 3. **CORS**: The Angular dev server runs on `localhost:4200` and the API on (typically) `localhost:5XXX`. Should I wire up a CORS policy in the API to allow this origin during development?

---

## Verification Plan

### Automated
- `dotnet build` must succeed with zero errors.
- `ng build --configuration development` must succeed.

### Manual
1. Start API: `dotnet run --project FocusTrack.Api`
2. Start UI: `ng serve` inside `focus-track-ui/`
3. Navigate to `http://localhost:4200/register` → fill form → submit.
4. Navigate to `http://localhost:4200/login` → fill form → receive JWT in browser storage.
5. Use the JWT to hit `GET /api/test/protected` and confirm `200 OK`.
