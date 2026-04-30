# Task API

A full-stack task management application — a RESTful .NET 8 backend with a React frontend.

## Technology Stack

**Backend**
- C# / ASP.NET Core 8 Web API
- SQLite with Entity Framework Core (`EnsureCreated`, no migrations)
- xUnit for testing
- Docker

**Frontend**
- React 19 + TypeScript + Vite
- lucide-react for icons
- Vitest + React Testing Library
- Proxied to the backend via Vite dev server (dev) / Nginx reverse proxy (Docker)

## Features

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
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Models/
│   ├── DTOs/
│   ├── Middleware/
│   ├── Exceptions/
│   └── taskflow.db             # SQLite file (auto-created on first run)
├── TaskApi.Tests/              # xUnit tests
├── frontend/                   # React app
│   ├── src/
│   │   ├── App.tsx             # All state and composition
│   │   ├── api/taskApi.ts      # Typed fetch wrapper
│   │   ├── types/task.ts       # Shared TypeScript types
│   │   └── components/         # Presentational components
│   ├── vite.config.ts          # Proxies /api → http://localhost:5255 (dev)
│   ├── nginx.conf              # Proxies /api → http://taskapi:8080 (Docker)
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

### Run the frontend

```bash
cd frontend
npm install
npm run dev
# App available at http://localhost:3000
```

The Vite dev server proxies all `/api` requests to `http://localhost:5255`, so no CORS configuration is needed during development. Swagger UI is available at `http://localhost:5255/swagger`.

### Docker (API + Frontend together)

```bash
docker-compose up -d
# Frontend at http://localhost:3000
# API at http://localhost:8080
# Health check: http://localhost:8080/health/ready
```

Nginx (port 3000) proxies `/api/` to the backend container. The frontend waits for the backend health check before starting.

## API Endpoints

### Tasks — `/api/tasks`

| Method | Endpoint        | Description                                        |
|--------|-----------------|----------------------------------------------------|
| GET    | `/api/tasks`    | Get tasks; optional `?status=all\|active\|completed\|overdue` |
| GET    | `/api/tasks/{id}` | Get task by ID                                  |
| POST   | `/api/tasks`    | Create a task                                      |
| PUT    | `/api/tasks/{id}` | Update a task                                   |
| DELETE | `/api/tasks/{id}` | Delete a task                                   |

### Health

| Method | Endpoint        | Description      |
|--------|-----------------|------------------|
| GET    | `/health/live`  | Liveness probe   |
| GET    | `/health/ready` | Readiness probe  |

## API Examples

### Create a task

```bash
curl -X POST http://localhost:5255/api/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Finish report",
    "priority": "high",
    "category": "Work",
    "dueDate": "2026-05-10T00:00:00Z"
  }'
```

### Get active tasks

```bash
curl "http://localhost:5255/api/tasks?status=active"
```

### Complete a task

```bash
curl -X PUT http://localhost:5255/api/tasks/{id} \
  -H "Content-Type: application/json" \
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
  "message": "Task with ID {id} not found",
  "timestamp": "2026-04-30T10:30:00Z",
  "traceId": "00-abc123-def456-00",
  "validationErrors": null
}
```

| Status | errorCode                | Trigger                          |
|--------|--------------------------|----------------------------------|
| 400    | `VALIDATION_ERROR`       | Invalid input or status filter   |
| 404    | `NOT_FOUND`              | Task ID not found                |
| 500    | `INTERNAL_SERVER_ERROR`  | Unhandled exception              |

## Configuration

### Production CORS

```json
{
  "AllowedOrigins": [
    "https://your-frontend-domain.com"
  ]
}
```

### Database

`taskflow.db` is created automatically via `EnsureCreated()` on startup. There are no EF migrations — if you add columns to the model, delete `taskflow.db` and let it recreate.

## Testing

```bash
dotnet test

# Single class or method
dotnet test --filter "FullyQualifiedName~TaskServiceTests"

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

Backend tests use **Moq** for service and controller layers; repository tests use the EF Core in-memory provider. Frontend tests use Vitest + React Testing Library.
