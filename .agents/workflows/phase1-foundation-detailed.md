---
description: 
---

# Phase 1: Foundation & Auth (Strict Execution)

**Step 1: Backend Initialization**
* Execute: `dotnet new webapi -n FocusTrack.Api --use-controllers`
* Execute: `dotnet add package Microsoft.EntityFrameworkCore.Design`
* Execute: `dotnet add package Oracle.EntityFrameworkCore`
* Execute: `dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer`
* Create directory structure: `/Models`, `/Data`, `/Services`, `/Controllers`, `/DTOs`.

**Step 2: Database Schema & EF Core Setup**
* In `/Models`, create `User.cs`: `Guid Id, string Email, string PasswordHash, DateTime CreatedAt`.
* In `/Models`, create `Session.cs`: `Guid Id, Guid UserId, string AppName, string WindowTitle, DateTime StartTime, DateTime EndTime, bool IsBrowser`.
* In `/Data`, create `FocusDbContext.cs` extending `DbContext`. Add `DbSet<User>` and `DbSet<Session>`.
* In `Program.cs`, register `FocusDbContext` using a placeholder connection string.

**Step 3: Authentication Controller & Service**
* In `/Services`, create `AuthService.cs` with methods: `RegisterAsync(email, password)` and `LoginAsync(email, password)`.
* Implement password hashing using `BCrypt.Net-Next` (add package).
* In `/Controllers`, create `AuthController.cs`. Inject `AuthService` and create `[HttpPost("register")]` and `[HttpPost("login")]` action methods returning standard JWTs.
* In `Program.cs`, configure JWT Bearer authentication. Ensure `builder.Services.AddControllers()` and `app.MapControllers()` are present.

**Step 4: Frontend Initialization**
* Execute: `ng new focus-track-ui --standalone true --routing true --style scss`
* Execute: `ng generate component features/auth/login`
* Execute: `ng generate component features/auth/register`
* Execute: `ng generate service core/auth/auth`
* Configure `app.routes.ts` to map `/login` and `/register`.

**Step 5: Integration & Acceptance**
* Create an HttpInterceptor in Angular to attach the JWT to all outgoing requests.
* **Acceptance Criteria:** A user must be able to register via the Angular UI, log in, receive a JWT, and successfully hit a test `[Authorize]` endpoint in a test Controller.