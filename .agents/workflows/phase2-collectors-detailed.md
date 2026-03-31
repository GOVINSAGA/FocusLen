---
description: 
---

# Phase 2: Data Collection Agents (Strict Execution)

**Step 1: Unified Ingestion Controller (.NET)**
* In `/DTOs`, create `ActivityPayloadDto.cs`: `string AppOrDomain, string WindowTitle, int DurationSeconds, string Source, DateTime Timestamp`.
* In `/Controllers`, create `ActivityController.cs`. Create an `[HttpPost]` action method protected by `[Authorize]` that accepts the DTO.
* In `/Services`, create `ActivityIngestionService.cs` to handle the logic of inserting incoming payloads into the `SESSIONS` Oracle table via EF Core.

**Step 2: Chrome Extension (Browser Agent)**
* Create directory `/agents/browser-extension`.
* Create `manifest.json` (Manifest V3). Required permissions: `tabs`, `storage`, `alarms`. Host permissions: `*://*/*`.
* Create `background.js`. Implement listeners for `chrome.tabs.onActivated`, `chrome.tabs.onUpdated`, and `chrome.windows.onFocusChanged`.
* **Logic:** Track active tab start time. On tab switch, calculate duration, read JWT from `chrome.storage.local`, and `fetch()` POST to `https://localhost:5001/api/activity`.

**Step 3: C# Windows Service (Desktop Agent)**
* Execute: `dotnet new worker -n FocusTrack.DesktopAgent` in a new directory `/agents/desktop-agent`.
* Execute: `dotnet add package Microsoft.Extensions.Hosting.WindowsServices`.
* Create `NativeMethods.cs`. Use `[DllImport("user32.dll")]` to import `GetForegroundWindow` and `GetWindowThreadProcessId`. 
* In `Worker.cs`, implement a `Task.Delay(5000)` polling loop.
* **Logic:** If the active process is `chrome.exe` or `msedge.exe`, ignore it. Otherwise, POST payload to `/api/activity` using `HttpClient` with a Bearer token.

**Step 4: Integration & Acceptance**
* **Acceptance Criteria:** 1. The Chrome extension successfully logs time spent on a domain to the Oracle database. 2. The Windows Service logs desktop app usage. 3. Unauthenticated requests are rejected with `401 Unauthorized`.