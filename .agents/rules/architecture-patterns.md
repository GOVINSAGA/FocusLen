---
trigger: glob
globs: *.cs
---

# .NET Architecture Rules (Controllers)
* **API Structure:** Use traditional Controller-based routing. All controllers must reside in the `/Controllers` folder, inherit from `ControllerBase`, and be decorated with `[ApiController]` and `[Route("api/[controller]")]`.
* **Separation of Concerns:** Controllers must ONLY handle HTTP routing, request validation, and returning `IActionResult`. All database interactions and business logic MUST be delegated to classes in the `/Services` folder.
* **Dependency Injection:** Inject required services into the Controller constructor. 
* **Data Transfer Objects (DTOs):** Controllers should never return raw EF Core database models. Always map database entities to DTOs in a `/DTOs` folder before returning them to the frontend.