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
dotnet test --filter "FullyQualifiedName~TaskServiceTests.CreateTaskAsync_CreatesTaskWithCorrectFields"

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
npm run dev        # dev server on http://localhost:3000 (proxies /api and /auth → http://localhost:5255)
npm run build      # tsc + vite build
npm run lint       # eslint
npm run test       # vitest watch mode
npm run test:run   # vitest single run (CI)
```

## Architecture

### Backend — `TaskApi/`

Three-layer: **Controller → Service → Repository → EF Core → SQLite**

- **Controllers/** — HTTP only; no business logic; returns typed DTOs. All task endpoints require `[Authorize]`.
- **Services/** — all business logic; validates `status` filter; throws `NotFoundException`/`ValidationException`
- **Repositories/** — EF Core queries; results ordered by `CreatedAt` ascending
- **Models/** — `TaskItem` (task entity) and `User` (auth entity); both live in `AppDbContext`
- **DTOs/** — `CreateTaskDto`, `UpdateTaskDto`, `TaskResponseDto`, `ErrorResponseDto`, `LoginDto`, `RegisterDto`, `AuthResponseDto`
- **Exceptions/** — `NotFoundException` → 404; `ValidationException` → 400 with `ValidationErrors` dict
- **Middleware/GlobalExceptionMiddleware.cs** — catches all exceptions; maps to `ErrorResponseDto` with `traceId` from `Activity.Current`
- **Settings/JwtSettings.cs** — bound from `appsettings.json` `JwtSettings` section (`SecretKey`, `Issuer`, `Audience`, `ExpirationMinutes`)

**Valid `status` filter values:** `all`, `active`, `completed`, `overdue`. The `overdue` filter is applied in-memory in the service (tasks where `IsCompleted=false` and `DueDate < UtcNow`).

**Database:** SQLite file `TaskApi/taskflow.db`. EF Core migrations live in `TaskApi/Migrations/`. The app calls `Database.Migrate()` on startup to apply pending migrations automatically.

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

### Auth layer

Auth is handled by `AuthController` (route prefix `/auth`, no `[Authorize]`) and `AuthService`.

- `POST /auth/register` — creates a `User`, hashes password with BCrypt, returns a JWT
- `POST /auth/login` — verifies credentials with timing-safe BCrypt comparison, returns a JWT

**`AuthService` is the one exception to the repository pattern** — it queries `AppDbContext.Users` directly rather than through a repository interface.

The JWT payload carries `ClaimTypes.NameIdentifier` = `user.Id` (int). `TasksController.GetUserId()` parses this claim to scope all task queries to the authenticated user. `TaskItem.UserId` is the foreign key enforcing this isolation at the DB level (cascade delete).

`appsettings.json` must contain:
```json
"JwtSettings": {
  "SecretKey": "<at least 32 chars>",
  "Issuer": "TaskApi",
  "Audience": "TaskApiUsers",
  "ExpirationMinutes": 60
}
```

---

### Frontend — `frontend/`

React 19 + TypeScript + Vite + React Router. Routing is handled at the root in `App.tsx`:

| Route | Component | Guard |
|-------|-----------|-------|
| `/` | `TaskApp` | `ProtectedRoute` |
| `/login` | `LoginPage` | — |
| `/register` | `RegisterPage` | — |

`ProtectedRoute` checks for `auth_token` in `localStorage`; absent → redirect to `/login`. On 401 responses, `taskApi.ts` clears the token and redirects.

All task state, filtering, sorting, and API calls live in `TaskApp` (inside `App.tsx`); components are presentational.

**Key files:**
- `src/App.tsx` — router, `TaskApp` (all task state, filtering/sorting logic, API calls, toast trigger, logout)
- `src/api/taskApi.ts` — authenticated fetch wrapper; reads `auth_token` from localStorage; base URL is `/api`
- `src/api/authApi.ts` — unauthenticated fetch wrapper; base URL is `/auth`
- `src/types/task.ts` — `Task` (`id: number`), `CreateTaskPayload`, `UpdateTaskPayload`, `Priority`, `TabFilter`
- `src/types/auth.ts` — `AuthResponse`, `LoginPayload`, `RegisterPayload`
- `src/pages/` — `LoginPage`, `RegisterPage`
- `src/components/ProtectedRoute.tsx` — token-presence guard

**Filtering & sorting** is done client-side in `App.tsx` over the full task list fetched once on mount. `overdue` is computed as `!isCompleted && dueDate < now`.

**Icons** use `lucide-react` throughout — no emoji or custom SVGs.

**Vite dev proxy** (`vite.config.ts`): both `/api/*` and `/auth/*` proxy to `http://localhost:5255`.

---

### Testing — `TaskApi.Tests/`

xUnit + **Moq**. Four test files:
- `Services/TaskServiceTests.cs` — mocks `ITaskRepository` and `ILogger<TaskService>`
- `Services/AuthServiceTests.cs` — uses EF Core in-memory provider; tests BCrypt hashing and JWT generation
- `Controllers/TasksControllerTests.cs` — mocks `ITaskService` and `ILogger<TasksController>`; manually sets `ClaimsPrincipal` with `NameIdentifier = "1"`
- `Repositories/TaskRepositoryTests.cs` — uses EF Core in-memory provider; each test gets a fresh DB via `Guid.NewGuid()` database name

Frontend tests use **Vitest + React Testing Library** (`src/__tests__/`); one file per component.
