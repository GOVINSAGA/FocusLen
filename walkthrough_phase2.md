# Phase 2: Data Collection Agents - Walkthrough

All three components for the telemetry ingestion pipeline have been successfully built! The backend is ready to accept data, the browser extension tracks tabs, and the Windows service tracks desktop apps.

## 1. Backend Ingestion API
I've updated the `FocusTrack.Api` project with the following:
* **`ActivityPayloadDto.cs`**: Defines the incoming data shape (`AppOrDomain`, `DurationSeconds`, `Source`, etc.).
* **`ActivityIngestionService.cs`**: Handles the math. You pass in `Timestamp` and `Duration`, and it automatically backwards-calculates `StartTime = Timestamp - Duration` and `EndTime = Timestamp` before saving it to the SQLite `Sessions` table.
* **`ActivityController.cs`**: Exposes `POST /api/activity`. It requires a `Bearer` token (`[Authorize]`) and parses the authenticated User's ID directly from the JWT claims to link the telemetry data securely.

## 2. Browser Agent (Chrome Extension)
Located in `e:\Learning Projects\AntiGravityTest1\agents\browser-extension`.
* **The Logic**: The service worker (`background.js`) tracks when you switch tabs or minimize Google Chrome. It accurately calculates duration. Any tab observed for less than 1 second is discarded.
* **The Authentication**: Clicking the extension icon opens `popup.html`. Type in the Email and Password you registered with. The popup calls the backend `/api/auth/login`, grabs your JWT, and injects it into `chrome.storage.local`. The background worker uses this token for all API uploads.

### How to install it:
1. Open Google Chrome and go to `chrome://extensions/`
2. Turn on **Developer mode** (top right corner).
3. Click **Load unpacked** (top left corner) and select the `agents\browser-extension` folder.
4. Pin the extension to your toolbar, click it, and authorize it using your Email & Password.

## 3. Desktop Agent (Windows Service)
Located in `e:\Learning Projects\AntiGravityTest1\agents\FocusTrack.DesktopAgent`.
* **The Logic**: Every 5 seconds, an invisible background thread queries the Windows OS via P/Invoke (`user32.dll`) to find out what application is in the foreground. If the application is Chrome or Edge, it ignores it (so we don't double count!). For any other desktop app (e.g., VS Code, Word), it tracks duration and POSTs it.
* **The Authentication**: In `agents/FocusTrack.DesktopAgent/appsettings.json`, I've set up placeholders:
  ```json
  "AgentAuth": {
    "Email": "agent@focustrack.io", // Change to YOUR email
    "Password": "password123"       // Change to YOUR password
  }
  ```
  On startup, the worker service reads these, logs in, gets a JWT, and holds it in memory permanently.

### How to run it:
1. Open a new terminal.
2. `cd "e:\Learning Projects\AntiGravityTest1\agents\FocusTrack.DesktopAgent"`
3. Edit `appsettings.json` to have your real email/password.
4. Run `dotnet run` to see it poll your active windows in real-time.

> [!CAUTION]
> Because we modified the `FocusTrack.Api` project with new code, you MUST restart your running `dotnet run` terminal in the API folder (`Ctrl+C`, then `dotnet run` again) to compile and expose the new `/api/activity` endpoint!
