# FocusTrack – Phase 3: Dashboard & Analytics

## Overview
This phase introduces data visualization and third-party authentication. We will build a backend analytics engine to aggregate session data, an Angular dashboard utilizing `Chart.js` for dynamic visualization, and integrate Google OAuth into the existing `.NET` JWT pipeline.

---

## User Review Required

> [!IMPORTANT]
> **Google OAuth Credentials**: Step 3 requires a Google **Client ID** and **Client Secret**. I will set up the backend logic and put placeholder values in `appsettings.Development.json`. You will need to create dummy or real credentials in the [Google Cloud Console](https://console.cloud.google.com/) and paste them into your config for the "Login with Google" button to actually work during verification.

> [!WARNING]
> **Chart.js vs ng2-charts**: Angular 17+ standalone components sometimes have strict dependency requirements for wrapper libraries like `ng2-charts`. If we encounter version conflicts, I will use native `Chart.js` directly within Angular `OnInit`/`ViewChild`, which is perfectly stable.

---

## Proposed Changes

### 1. Backend Analytics (.NET)

#### [NEW] `FocusTrack.Api/DTOs/AnalyticsDtos.cs`
Defines `DailyUsageDto` (Date, TotalSeconds) and `TopAppDto` (AppName, TotalDurationSeconds).

#### [NEW] `FocusTrack.Api/Services/AnalyticsService.cs`
Implements raw EF Core LINQ grouping:
- **GetDailyUsage(userId, daysBack)**: Groups `Sessions` by the Date component of `StartTime` over the last `N` days. 
- **GetTopApps(userId, limit)**: Groups by `AppName`, sums the durations (EndTime - StartTime), and returns the top `N`.

#### [NEW] `FocusTrack.Api/Controllers/AnalyticsController.cs`
Exposes `GET /api/analytics/daily` and `GET /api/analytics/top-apps` protected by `[Authorize]`. Injects `AnalyticsService`.

---

### 2. Frontend Analytics Dashboard (Angular)

#### [MODIFY] `package.json`
Execute `npm install chart.js ng2-charts`.

#### [NEW] `src/app/core/analytics/analytics.ts`
Angular Service to `HttpClient.get()` the backend `/api/analytics/*` endpoints.

#### [NEW] `src/app/features/dashboard/overview/`
The actual reporting view.
- Contains a visually stunning grid layout using Tailwind.
- Renders a **Bar Chart** mapping out the last 7 days of raw tracking.
- Renders a **Doughnut/Pie Chart** showing the user's top 5 most used apps.

#### [MODIFY] `src/app/app.routes.ts`
Route `/dashboard` point to `OverviewComponent` instead of the generic placeholder we built in Phase 2.

---

### 3. Google OAuth Integration

#### [MODIFY] `FocusTrack.Api.csproj`
Add package: `Microsoft.AspNetCore.Authentication.Google`.

#### [MODIFY] `FocusTrack.Api/Program.cs`
Inject `.AddGoogle(options => ...)` into the authentication builder.

#### [MODIFY] `FocusTrack.Api/Controllers/AuthController.cs`
Add a dedicated endpoint for issuing a Google OAuth challenge, and a callback endpoint `GET /api/auth/google-callback`.
If the user's email doesn't exist in `Users`, we automatically create a new account for them. We then issue our standard FocusTrack JWT so the rest of the app's interceptors work perfectly.

#### [MODIFY] `src/app/features/auth/login/login.html` & `.ts`
Add a polished "Sign in with Google" button beneath the traditional login form. Clicking this will redirect the browser directly to `http://localhost:5028/api/auth/google-login`.

---

## Open Questions

1. **Angular Dashboard Routing**: Right now, `/dashboard` is a blank page that just has a "Logout" button. I will convert it to be the `OverviewComponent` hosting the charts. Are you okay with the charts being the main `/dashboard` landing page?
2. **Google OAuth Config**: To make Google Login work locally, we need to provide a redirect URI to Google. I'll configure `.NET` to use `http://localhost:5028/signin-google`. Is `5028` still the exact port your API is running on locally?
