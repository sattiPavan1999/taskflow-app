# Task API — Project Summary

## Status: Build passing · 36/36 tests passing

## Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Language | C# | latest |
| Framework | .NET / ASP.NET Core | 8.0 |
| Database | SQLite | — |
| ORM | Entity Framework Core | 8.0.10 |
| Authentication | JWT Bearer + BCrypt | 8.0.10 / 4.0.3 |
| Testing | xUnit + Moq | 2.9.3 / 4.20.72 |
| Frontend | React + TypeScript + Vite | 19 / 6.0 / 8.0 |
| Routing | React Router | v7 |
| Container | Docker + docker-compose | — |

## Architecture

**Backend — Controller → Service → Repository → EF Core → SQLite**

- Controllers: no business logic; all task endpoints protected by `[Authorize]`
- Services: business logic, status-filter validation, exception throwing
- Repositories: EF Core queries; results ordered by `CreatedAt` ascending
- `AuthService` is the exception — it accesses `AppDbContext.Users` directly (no repo layer)
- Global exception middleware maps `NotFoundException` → 404, `ValidationException` → 400
- JWT claims carry `int` user ID; all task queries are scoped to the authenticated user

**Frontend — React SPA with React Router**

- Three routes: `/` (protected task dashboard), `/login`, `/register`
- `ProtectedRoute` checks `localStorage.auth_token`; absent → redirect `/login`
- All state in `App.tsx` (`TaskApp`); components are presentational
- Client-side filtering and sorting over the full task list
- `taskApi.ts` sends `Authorization: Bearer` header on every request; redirects to `/login` on 401

## API Surface

### Auth (`/auth` — public)
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/auth/register` | Create account; returns JWT |
| POST | `/auth/login` | Authenticate; returns JWT |

### Tasks (`/api/tasks` — JWT required)
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/tasks?status=` | List tasks (`all`/`active`/`completed`/`overdue`) |
| GET | `/api/tasks/{id}` | Get single task |
| POST | `/api/tasks` | Create task |
| PUT | `/api/tasks/{id}` | Update task |
| DELETE | `/api/tasks/{id}` | Delete task |

### Health
| GET `/health/live` | GET `/health/ready` |
|--------------------|---------------------|
| Liveness probe | Readiness probe (DB check) |

## Test Coverage

| Suite | File | Tests |
|-------|------|-------|
| Service — tasks | `TaskServiceTests.cs` | 9 |
| Service — auth | `AuthServiceTests.cs` | 5 |
| Controller | `TasksControllerTests.cs` | 10 |
| Repository | `TaskRepositoryTests.cs` | 12 |
| **Total** | | **36** |

- Controller + service tests use **Moq** mocks
- Repository + auth service tests use the **EF Core in-memory** provider
- Each in-memory test gets an isolated database (`Guid.NewGuid()` name)

## Key Decisions

- **`int` primary keys** — `TaskItem.Id` and `User.Id` are auto-increment integers (EF Core `AUTOINCREMENT` on SQLite). Previously `Guid`.
- **Per-user isolation** — `TaskItem.UserId` foreign key with cascade delete; service layer always passes `userId` from JWT claims down to the repository.
- **Timing-safe login** — `AuthService.LoginAsync` always runs `BCrypt.Verify` (even for unknown emails) to prevent user-enumeration via timing.
- **Single migration** — Migrations directory contains one `InitialCreate` covering both `Users` and `Tasks` tables.
- **No controller interfaces** — interfaces exist at the service and repository layers only; controller tests instantiate the concrete class directly.
