---
trigger: glob
globs: *.cs, *.ts, *.html, *.sql
---

# FocusTrack Stack Conventions
* **Backend:** Use .NET 8 Web API with Controllers. Use Entity Framework (EF) Core with the Oracle provider for database interactions.
* **Frontend:** Use Angular 17+ with standalone components. Use strict TypeScript typing.
* **Database:** Oracle SQL. Ensure all tables have proper primary keys and indexing on timestamp columns. 
* **Modularity:** Separate controllers, services, repositories, and DTOs in the .NET backend.