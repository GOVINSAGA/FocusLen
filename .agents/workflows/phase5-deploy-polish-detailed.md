---
description: 
---

# Phase 5: Deploy & Polish (Strict Execution)

**Step 1: Containerization**
* Create `Dockerfile` in the `.NET` root. Use multi-stage build: `mcr.microsoft.com/dotnet/sdk:8.0` for build, `aspnet:8.0` for runtime. Expose port 8080.
* Create `Dockerfile` in the `Angular` root. Use multi-stage build: `node:20-alpine` for build, `nginx:alpine` to serve static files on port 80.
* Create `docker-compose.yml` in the project root to orchestrate both containers. Pass environment variables for Oracle connection strings and JWT keys.

**Step 2: CI/CD Pipeline**
* Create `.github/workflows/deploy.yml`.
* Configure steps: `actions/checkout@v4`, `actions/setup-dotnet@v4`, `actions/setup-node@v4`.
* Add steps to build the code and build the Docker images to verify successful compilation on push to `main`.

**Step 3: OCI Server Configuration**
* Create `/deploy/nginx.conf`. Configure as a reverse proxy: route `/api/*` to the .NET container, and `/` to the Angular container.
* Create `/deploy/setup.sh` containing commands to install Docker, pull the repo, and run `docker-compose up -d`.

**Step 4: Integration & Acceptance**
* **Acceptance Criteria:** 1. `docker-compose up --build` locally starts both the frontend and backend. 2. The containerized backend successfully connects to the remote Oracle Database.