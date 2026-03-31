# FocusTrack Phase 1 – Task Checklist

## Step 1: Backend Initialization
- `[ ]` `dotnet new webapi -n FocusTrack.Api --use-controllers`
- `[ ]` Add NuGet: `Microsoft.EntityFrameworkCore.Design`
- `[ ]` Add NuGet: `Microsoft.EntityFrameworkCore.Sqlite` *(SQLite for local dev)*
- `[ ]` Add NuGet: `Microsoft.AspNetCore.Authentication.JwtBearer`
- `[ ]` Add NuGet: `BCrypt.Net-Next`
- `[ ]` Create directories: `/Models`, `/Data`, `/Services`, `/Controllers`, `/DTOs`

## Step 2: Database Schema & EF Core
- `[ ]` Create `Models/User.cs`
- `[ ]` Create `Models/Session.cs`
- `[ ]` Create `Data/FocusDbContext.cs`
- `[ ]` Register `FocusDbContext` in `Program.cs` (SQLite, from config)

## Step 3: Auth Controller & Service
- `[ ]` Create `DTOs/RegisterDto.cs`
- `[ ]` Create `DTOs/LoginDto.cs`
- `[ ]` Create `DTOs/AuthResponseDto.cs`
- `[ ]` Create `Services/AuthService.cs` (RegisterAsync, LoginAsync)
- `[ ]` Create `Controllers/AuthController.cs` (POST register, POST login)
- `[ ]` Create `Controllers/TestController.cs` ([Authorize] GET endpoint)
- `[ ]` Configure JWT Bearer + CORS in `Program.cs`

## Step 4: Frontend Initialization
- `[ ]` `ng new focus-track-ui --standalone --routing --style scss`
- `[ ]` `ng generate component features/auth/login`
- `[ ]` `ng generate component features/auth/register`
- `[ ]` `ng generate service core/auth/auth`
- `[ ]` Configure `app.routes.ts`

## Step 5: Integration & Polish
- `[ ]` Create `core/interceptors/auth.interceptor.ts`
- `[ ]` Wire up `HttpClient` + interceptor in `app.config.ts`
- `[ ]` Build basic Login/Register HTML templates

## Step 6: Verification
- `[ ]` `dotnet build` – zero errors
- `[ ]` `ng build --configuration development` – zero errors
