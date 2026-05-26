# ContentHub

ContentHub is a .NET 10 content management API. Version 1.0 establishes the backend foundation for managing categories, authors, posts, assets, audit logs, search, notifications, authentication, and authorization.

## Version 1.0 Scope

- Solution skeleton with API, Application, Contracts, Data, Domain, Infrastructure, Unit Tests, Integration Tests, and Architecture Tests projects
- Baseline NuGet packages for ASP.NET Core, Entity Framework Core, PostgreSQL, JWT auth, validation, testing, and architecture rules
- Minimal API foundation with endpoint definitions, validation filters, middleware, health checks, and Swagger
- PostgreSQL database foundation with EF Core DbContext, entities, configurations, migrations, and seed data
- Docker Compose PostgreSQL setup for local development
- Authentication with registration, login, logout, refresh tokens, current user lookup, password hashing, and JWT generation
- Role-based authorization policies for authenticated users, admins, editors, and authors
- Category management
- Author management
- Post creation, update, publishing, unpublishing, archiving, scheduling, featured posts, and public post reads
- Asset upload and local file storage
- Audit log recording and admin audit log access
- Basic search for posts, assets, and combined search
- Basic in-app notifications and notification preferences
- Integration test foundation with Testcontainers PostgreSQL
- Unit tests for validation, slugs, pagination, entities, and auth infrastructure
- Architecture tests for dependencies, endpoint rules, naming conventions, data rules, and vertical slice structure

## Tech Stack

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core 10
- PostgreSQL 16
- Npgsql
- JWT Bearer authentication
- FluentValidation
- xUnit
- FluentAssertions
- Testcontainers for PostgreSQL
- NetArchTest
- Docker Compose

## Solution Structure

```text
ContentHub.Api/                  API host, endpoints, middleware, auth/Swagger setup
ContentHub.Application/          Application abstractions and shared application concerns
ContentHub.Contracts/            Shared contracts project
ContentHub.Data/                 EF Core entities, DbContext, migrations, DTOs, seeders
ContentHub.Domain/               Domain-level abstractions
ContentHub.Infrastructure/       Authentication, storage, and infrastructure services
tests/ContentHub.UnitTests/      Unit tests
tests/ContentHub.IntegrationTests/ Integration/API tests with Testcontainers PostgreSQL
tests/ContentHub.ArchitectureTests/ Architecture and dependency rule tests
```

## Prerequisites

- .NET 10 SDK
- Docker Desktop, required for PostgreSQL and integration tests
- PowerShell 7, optional but pinned in `dotnet-tools.json`

## Getting Started

Restore tools and packages:

```powershell
dotnet tool restore
dotnet restore
```

Start PostgreSQL:

```powershell
docker compose up -d
```

Run the API:

```powershell
dotnet run --project ContentHub.Api
```

The development launch profile exposes:

- HTTP: `http://localhost:5181`
- HTTPS: `https://localhost:7027`

Swagger is enabled in development.

## Configuration

Development configuration lives in:

```text
ContentHub.Api/appsettings.Development.json
```

Default development database connection:

```text
Host=localhost;Port=5432;Database=contenthub;Username=contenthub;Password=contenthub
```

Default seeded admin user:

```text
Email: admin@contenthub.local
Username: admin
Password: Admin123!
```

These values are development defaults only. Change secrets before deploying anywhere real.

## Database

The API applies EF Core migrations at startup and seeds baseline roles plus the development admin user.

Create a new migration:

```powershell
dotnet ef migrations add MigrationName --project ContentHub.Data --startup-project ContentHub.Api --context ContentHubDbContext
```

Apply migrations manually:

```powershell
dotnet ef database update --project ContentHub.Data --startup-project ContentHub.Api --context ContentHubDbContext
```

## Testing

Run all tests:

```powershell
dotnet test
```

Run without restore:

```powershell
dotnet test --no-restore
```

Current v1.0 verification:

```text
Total: 80
Passed: 80
Failed: 0
Skipped: 0
```

Breakdown:

- Unit tests: 43 passed
- Architecture tests: 22 passed
- Integration tests: 15 passed

Integration tests require Docker because they use Testcontainers PostgreSQL.

## API Areas

- `/api/auth/*`
- `/api/categories`
- `/api/authors`
- `/api/posts`
- `/api/public/posts`
- `/api/assets`
- `/api/search`
- `/api/notifications`
- `/api/audit-logs`
- `/health`, `/health/live`, `/health/ready`

## Notes for v1.0

- Authorization is role-policy based in v1.0. Permission constants exist, but full permission-based policy enforcement is not yet part of the current baseline.
- Asset storage is local filesystem storage under `storage/assets`.
- Search is basic database-backed search, not a dedicated search engine.
- Notifications are basic in-app notifications.
- EF Core currently logs warnings for required relationships where the required entity has a global query filter. Tests pass, but this is worth revisiting as the data model evolves.
- FluentAssertions v8 prints a license notice during test runs.

## License

No license has been added yet.
