---
trigger: glob
globs: *.cs, *.ts
---

# Global Error Handling
* **Backend (.NET):** Never leak raw exception stack traces to the frontend. Implement a Global Exception Handler middleware that intercepts errors and returns standardized JSON responses (e.g., `{ "error": "Message", "statusCode": 500 }`).
* **Frontend (Angular):** Implement an `HttpInterceptor` that catches 400/500 level errors from the API and displays a user-friendly toast notification.
* **Logging:** Use `ILogger<T>` in all .NET services. Log critical events and errors.