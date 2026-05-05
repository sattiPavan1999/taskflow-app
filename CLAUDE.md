# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Backend (ASP.NET Core)
```bash
# Run the API on http://localhost:5255
dotnet run --project TaskApi/TaskApi.csproj --launch-profile http

# Build
dotnet build

# Run all tests
dotnet test

# Run a single test class or method
dotnet test --filter "FullyQualifiedName~TaskServiceTests"
dotnet test --filter "FullyQualifiedName~TaskServiceTests.CreateTaskAsync_ShouldCreateNewTask"

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Docker — starts both API (port 8080) and frontend (port 3000)
docker-compose up -d
docker-compose up -d --build   # rebuild images
```

### Frontend (React + Vite)
```bash
cd frontend

npm install        # install dependencies
npm run dev        # dev server on http://localhost:3000 (proxies /api → http://localhost:5255)
npm run build      # tsc + vite build
npm run lint       # eslint
npm run test       # vitest watch mode
npm run test:run   # vitest single run (CI)
```

## Architecture

### Backend — `TaskApi/`

Three-layer: **Controller → Service → Repository → EF Core → SQLite**

- **Controllers/** — HTTP only; no business logic; returns typed DTOs
- **Services/** — all business logic; validates `status` filter; throws `NotFoundException`/`ValidationException`
- **Repositories/** — EF Core queries; results ordered by `CreatedAt` ascending
- **Models/TaskItem.cs** — entity with `Id`, `Title`, `Description`, `IsCompleted`, `CreatedAt`, `Priority`, `Category`, `DueDate`
- **DTOs/** — `CreateTaskDto`, `UpdateTaskDto`, `TaskResponseDto` (all include `Priority`, `Category`, `DueDate`), `ErrorResponseDto`
- **Exceptions/** — `NotFoundException` → 404; `ValidationException` → 400 with `ValidationErrors` dict
- **Middleware/GlobalExceptionMiddleware.cs** — catches all exceptions; maps to `ErrorResponseDto` with `traceId` from `Activity.Current`

**Valid `status` filter values:** `all`, `active`, `completed`, `overdue`. The `overdue` filter is applied in-memory in the service (tasks where `IsCompleted=false` and `DueDate < UtcNow`).

**Database:** SQLite file `TaskApi/taskflow.db`. EF Core migrations live in `TaskApi/Migrations/`. The app calls `Database.Migrate()` on startup to apply any pending migrations automatically.

**Migration workflow:**
```bash
# Add a new migration after changing the model
dotnet ef migrations add <MigrationName> --project TaskApi/TaskApi.csproj

# Apply migrations manually (also runs automatically on startup)
dotnet ef database update --project TaskApi/TaskApi.csproj

# Remove the last unapplied migration
dotnet ef migrations remove --project TaskApi/TaskApi.csproj
```

**Error responses** always use `ErrorResponseDto`: `{ errorCode, message, timestamp, traceId, validationErrors? }`.

**CORS:** development allows all origins; production reads `AllowedOrigins[]` from `appsettings.json`.

**Swagger UI:** available at `http://localhost:5255/swagger` in development only. An OpenAPI spec snapshot lives at `swagger/taskapi-openapi.yaml`.

**Docker port difference:** `dotnet run` binds to port 5255; the Docker image binds to 8080 (`ASPNETCORE_URLS=http://+:8080`). The Vite dev proxy targets 5255, Nginx targets 8080.

`Program` is `public partial class` to allow integration test access.

---

### Frontend — `frontend/`

React 19 + TypeScript + Vite. All state lives in `App.tsx`; components are presentational.

**Key files:**
- `src/App.tsx` — all task state, filtering/sorting logic, API calls, toast trigger
- `src/api/taskApi.ts` — typed fetch wrapper; base URL is `/api` (proxied by Vite in dev)
- `src/types/task.ts` — `Task`, `CreateTaskPayload`, `UpdateTaskPayload`, `Priority`, `TabFilter`
- `src/components/` — `Header`, `AddTodoForm`, `SearchBar`, `TabBar`, `TodoItem`, `EmptyState`, `ToastContainer`

**Filtering & sorting** is done client-side in `App.tsx` over the full task list fetched once on mount. `overdue` is computed as `!isCompleted && dueDate < now`.

**Icons** use `lucide-react` throughout — no emoji or custom SVGs.

**Vite dev proxy** (`vite.config.ts`): `/api/*` → `http://localhost:5255` so the frontend needs no absolute URL and avoids CORS in development.

---

### Testing — `TaskApi.Tests/`

xUnit + **Moq**. Three test files mirror the three layers:
- `Services/TaskServiceTests.cs` — mocks `ITaskRepository` and `ILogger<TaskService>` via `Mock<T>`
- `Controllers/TasksControllerTests.cs` — mocks `ITaskService` and `ILogger<TasksController>`
- `Repositories/TaskRepositoryTests.cs` — uses the EF Core in-memory provider for `AppDbContext` (no mocks)

Frontend tests use **Vitest + React Testing Library** (`src/__tests__/`); one file per component.
