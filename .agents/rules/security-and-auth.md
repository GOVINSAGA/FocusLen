---
trigger: always_on
---

# Auth & Security Guidelines
* Never hardcode secrets or database connection strings. Assume Oracle OCI Vault, environment variables, or local user secrets are being used.
* The application uses a dual-auth system: JWT (Access + Refresh tokens) and Google OAuth. 
* When generating API endpoints, ensure they are protected by the appropriate `[Authorize]` attributes.