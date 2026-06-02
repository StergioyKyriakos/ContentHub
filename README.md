# ContentHub

ContentHub is a .NET 10 content management API. Version 1.0 establishes the backend foundation for managing categories, authors, posts, assets, audit logs, search, notifications, authentication, and authorization.

## Version 1.1 Update

Version 1.1 expands the v1 backend with stronger account flows, broader endpoint coverage, and more complete operational visibility.

- Email verification flow with request and verify endpoints
- Forgot password and reset password flow
- Refresh token rotation and session/device management endpoints
- Improved validator messages across feature commands and queries
- Better seed data for local development and test scenarios
- Consistent command/query request body usage across feature endpoints
- Expanded integration test coverage for API feature endpoints
- Wider audit logging for user registration, login, category updates/deletes, author updates/deletes, post archive/schedule/delete/unfeature, and asset attach/detach operations
- Integration test response logging for easier API debugging in Rider and CLI test output

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

```
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

## Testing

v1.0 verification:

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

v1.1 verification:

```text
Total: 103
Passed: 103
Failed: 0
Skipped: 0
```

Breakdown:

- Unit tests: 43 passed
- Architecture tests: 22 passed
- Integration tests: 38 passed

The v1.1 integration suite requires Docker Desktop/Testcontainers to be available.

## API Areas

- `/api/auth/*`
- `/api/categories`
- `/api/authors`
- `/api/posts`
- `/api/public/posts`
- `/api/assets`
- `/api/search`
- `/api/notifications`
- `/api/admin/audit-logs`
- `/health`, `/health/live`, `/health/ready`

## Notes for v1.0 and v1.1

- Authorization is role-policy based in v1.0. Permission constants exist, but full permission-based policy enforcement is not yet part of the current baseline.
- Asset storage is local filesystem storage under `storage/assets`.
- Search is basic database-backed search, not a dedicated search engine.
- Notifications are basic in-app notifications.
- EF Core currently logs warnings for required relationships where the required entity has a global query filter. Tests pass, but this is worth revisiting as the data model evolves.
- FluentAssertions v8 prints a license notice during test runs.

## License

No license has been added yet.
