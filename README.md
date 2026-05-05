# Task API

A full-stack task management application — a RESTful .NET 8 backend with a React frontend and JWT authentication.

## Technology Stack

**Backend**
- C# / ASP.NET Core 8 Web API
- SQLite with Entity Framework Core (migrations)
- JWT Bearer authentication with BCrypt password hashing
- xUnit + Moq for testing
- Docker

**Frontend**
- React 19 + TypeScript + Vite
- React Router v7 for client-side routing
- lucide-react for icons
- Vitest + React Testing Library
- Proxied to the backend via Vite dev server (dev) / Nginx reverse proxy (Docker)

## Features

- JWT authentication — register, login, per-user task isolation
- Create, read, update, and delete tasks
- Task fields: title, description, priority (`low` / `medium` / `high`), category, due date
- Filter by status: `all`, `active`, `completed`, `overdue`
- Search and sort tasks client-side
- Light / dark mode toggle
- Toast notification on task completion
- Global exception handling with structured error responses and trace correlation
- Health check endpoints (liveness & readiness)
- CORS support
- Docker containerisation

## Project Structure

```
task_api/
├── TaskApi/                    # ASP.NET Core API
│   ├── Controllers/            # AuthController, TasksController, HealthController
│   ├── Services/               # IAuthService/AuthService, ITaskService/TaskService
│   ├── Repositories/           # ITaskRepository/TaskRepository
│   ├── Models/                 # TaskItem, User, AppDbContext
│   ├── DTOs/                   # Request/response DTOs for tasks and auth
│   ├── Middleware/             # GlobalExceptionMiddleware
│   ├── Exceptions/             # NotFoundException, ValidationException
│   ├── Settings/               # JwtSettings
│   ├── Migrations/             # EF Core migrations
│   └── taskflow.db             # SQLite file (auto-created on first run)
├── TaskApi.Tests/              # xUnit tests (36 tests)
├── frontend/                   # React app
│   ├── src/
│   │   ├── App.tsx             # Router, TaskApp (all state and composition)
│   │   ├── pages/              # LoginPage, RegisterPage
│   │   ├── api/                # taskApi.ts (authenticated), authApi.ts
│   │   ├── types/              # task.ts, auth.ts
│   │   └── components/         # Presentational components + ProtectedRoute
│   ├── vite.config.ts          # Proxies /api and /auth → http://localhost:5255 (dev)
│   ├── nginx.conf              # Proxies /api and /auth → http://taskapi:8080 (Docker)
│   └── Dockerfile              # Node build → Nginx serve
├── swagger/taskapi-openapi.yaml
├── Dockerfile
└── docker-compose.yml
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- Docker (optional)

### Run the backend

```bash
dotnet run --project TaskApi/TaskApi.csproj --launch-profile http
# API available at http://localhost:5255
# Swagger UI at http://localhost:5255/swagger
```

EF Core migrations are applied automatically on startup, creating `taskflow.db` if it doesn't exist.

### Run the frontend

```bash
cd frontend
npm install
npm run dev
# App available at http://localhost:3000
```

The Vite dev server proxies `/api` and `/auth` to `http://localhost:5255`.

### Docker (API + Frontend together)

```bash
docker-compose up -d
# Frontend at http://localhost:3000
# API at http://localhost:8080
# Health check: http://localhost:8080/health/ready
```

Nginx (port 3000) proxies `/api/` and `/auth/` to the backend container.

## API Endpoints

### Auth — `/auth`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/register` | Register a new user; returns JWT |
| POST | `/auth/login` | Login; returns JWT |

### Tasks — `/api/tasks` (JWT required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tasks` | Get tasks; optional `?status=all\|active\|completed\|overdue` |
| GET | `/api/tasks/{id}` | Get task by ID |
| POST | `/api/tasks` | Create a task |
| PUT | `/api/tasks/{id}` | Update a task |
| DELETE | `/api/tasks/{id}` | Delete a task |

### Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health/live` | Liveness probe |
| GET | `/health/ready` | Readiness probe |

## API Examples

### Register and get a token

```bash
curl -X POST http://localhost:5255/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "yourpassword"}'
# Returns: { "token": "<jwt>", "email": "user@example.com" }
```

### Create a task

```bash
curl -X POST http://localhost:5255/api/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <jwt>" \
  -d '{
    "title": "Finish report",
    "priority": "high",
    "category": "Work",
    "dueDate": "2026-05-10T00:00:00Z"
  }'
```

### Get active tasks

```bash
curl "http://localhost:5255/api/tasks?status=active" \
  -H "Authorization: Bearer <jwt>"
```

### Complete a task

```bash
curl -X PUT http://localhost:5255/api/tasks/{id} \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <jwt>" \
  -d '{
    "title": "Finish report",
    "isCompleted": true,
    "priority": "high",
    "category": "Work"
  }'
```

## Error Responses

All errors use a consistent shape:

```json
{
  "errorCode": "NOT_FOUND",
  "message": "Task with ID 42 not found",
  "timestamp": "2026-05-05T10:30:00Z",
  "traceId": "00-abc123-def456-00",
  "validationErrors": null
}
```

| Status | errorCode | Trigger |
|--------|-----------|---------|
| 400 | `VALIDATION_ERROR` | Invalid input, duplicate email, wrong credentials |
| 404 | `NOT_FOUND` | Task ID not found |
| 500 | `INTERNAL_SERVER_ERROR` | Unhandled exception |

## Configuration

### JWT (`appsettings.json`)

```json
"JwtSettings": {
  "SecretKey": "<at least 32 characters>",
  "Issuer": "TaskApi",
  "Audience": "TaskApiUsers",
  "ExpirationMinutes": 60
}
```

### Production CORS

```json
{
  "AllowedOrigins": ["https://your-frontend-domain.com"]
}
```

## Testing

```bash
dotnet test

# Single class or method
dotnet test --filter "FullyQualifiedName~TaskServiceTests"

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

Backend tests use **Moq** for service and controller layers; repository and auth service tests use the EF Core in-memory provider. Frontend tests use Vitest + React Testing Library.
