---
description: 
---

# Phase 4: Smart Features (Strict Execution)

**Step 1: Schema Expansion & Rules Engine**
* In `/Models`, create `AppLimit.cs`: `Guid Id, Guid UserId, string AppName, int DailyLimitMinutes`.
* Generate and apply an EF Core migration: `dotnet ef migrations add AddSmartFeatures`.
* In `/Services`, create `LimitsService.cs` to check today's session sum against the `AppLimit` table.
* In `/Controllers`, create `LimitsController.cs` with a `[HttpGet("alerts")]` action method.

**Step 2: Background Jobs & Emails (.NET)**
* Execute: `dotnet add package Hangfire.Core` and `dotnet add package Hangfire.MemoryStorage`.
* Execute: `dotnet add package MailKit`.
* In `/Services`, create `EmailService.cs` using MailKit to send SMTP emails.
* In `/Services`, create `WeeklyReportJob.cs` to calculate the week's summary and format an HTML email.
* In `Program.cs`, configure Hangfire to trigger `WeeklyReportJob.Execute()` via a CRON expression.

**Step 3: Frontend Settings UI**
* Execute: `ng generate component features/settings/preferences --standalone`
* Build a Tailwind-styled UI form allowing users to add an app name and set a time limit.

**Step 4: Integration & Acceptance**
* **Acceptance Criteria:** 1. User can set a limit via the UI. 2. After simulated usage, the `/api/limits/alerts` endpoint returns a "Limit Exceeded" flag. 3. Hangfire dashboard is accessible and shows the scheduled email job.