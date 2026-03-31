---
description: 
---

# Phase 3: Dashboard & Analytics (Strict Execution)

**Step 1: Backend Analytics Controller (.NET)**
* In `/DTOs`, create `DailyUsageDto.cs` and `TopAppDto.cs`.
* In `/Services`, create `AnalyticsService.cs`. Write EF Core LINQ queries using `.AsNoTracking()`, `.GroupBy(s => s.AppName)`, and `.Sum(s => s.DurationSeconds)` to aggregate data.
* In `/Controllers`, create `AnalyticsController.cs`. Create a `[HttpGet("summary")]` action method that calls the service and returns the DTOs.

**Step 2: Frontend Dashboard (Angular)**
* Execute: `npm install chart.js ng2-charts`
* Execute: `ng generate component features/dashboard/overview --standalone`
* Execute: `ng generate service core/analytics/analytics`
* In the `OverviewComponent`, implement a Donut Chart (Top 5 apps) and a Bar Chart (total screen time per day over 7 days) using Tailwind for layout.

**Step 3: Google OAuth Integration**
* Execute: `dotnet add package Microsoft.AspNetCore.Authentication.Google`.
* Update `Program.cs` to add `.AddGoogle()`. 
* Update `AuthController.cs` with a `[HttpGet("google-login")]` endpoint that issues a challenge and handles the callback to generate a standard JWT for the frontend.
* Update the Angular Login component to include a "Login with Google" button.

**Step 4: Integration & Acceptance**
* **Acceptance Criteria:** 1. User can log in via Google OAuth. 2. The Angular dashboard successfully fetches aggregation data and renders charts dynamically based on the DB data.