# ContentHub

ContentHub is a .NET 10 content management API. The current backend includes content management, authentication, authorization, audit logs, search, notifications, Redis infrastructure, background jobs, cloud-ready storage, and production-style Docker Compose support.

## Version 1.0 

- Solution skeleton with API, Application, Contracts, Data, Domain, Infrastructure, Unit Tests, Integration Tests, and Architecture Tests projects
- Baseline NuGet packages for ASP.NET Core, Entity Framework Core, PostgreSQL, JWT auth, validation, testing, and architecture rules
- Minimal API foundation with endpoint definitions, validation filters, middleware, health checks, and Swagger
- PostgreSQL database foundation with EF Core DbContext, entities, configurations, migrations, and seed data
- Docker Compose PostgreSQL setup for local development
- Authentication with registration, login, logout, refresh tokens, current user lookup, password hashing, and JWT generation
- Role-based authorization policies for authenticated users, admins, editors, and authors
- Category management
- Author management
- Post creation, update, publishing, unpublishing, archiving, featured posts, and public post reads
- Asset upload and local file storage
- Audit log recording and admin audit log access
- Basic search for posts, assets, and combined search
- Basic in-app notifications and notification preferences
- Integration test foundation with Testcontainers PostgreSQL
- Unit tests for validation, slugs, pagination, entities, and auth infrastructure
- Architecture tests for dependencies, endpoint rules, naming conventions, data rules, and vertical slice structure


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

## Version 2.1 Redis Infrastructure

Version 2.1 starts the production infrastructure track with Redis-backed caching and rate limiting.

- Added Redis service to Docker Compose
- Added `docker/redis/redis.conf` for local Redis configuration
- Added application abstractions for caching and rate limiting
- Added Redis cache service and Redis-backed rate limit service
- Added login throttling for repeated failed login attempts
- Added caching for `/api/public/featured-posts`
- Added featured-post cache invalidation when post visibility, publish state, featured state, or featured post content changes

## Version 2.2 Background Jobs

Version 2.2 adds hosted background jobs for recurring backend work.

- Added hosted background job options under `BackgroundJobs`
- Added scheduled post publisher job
- Added notification delivery job for in-app notification deliveries
- Added expired token cleanup job and cleanup service
- Added Redis-backed distributed locks when `BackgroundJobs:RequireDistributedLock` is enabled
- Added integration coverage for expired token cleanup behavior

## Version 2.3 Scheduled Posts

Version 2.3 makes scheduled posts work end to end.

- Added working `POST /api/posts/{id}/schedule` flow
- Scheduled posts use `Scheduled` status until their scheduled time arrives
- The scheduled post publisher job publishes due posts automatically
- Published scheduled posts create audit logs and in-app notifications
- Featured post cache is invalidated when scheduled publishing changes public content
- Public post endpoints show scheduled posts only after they are published

## Version 2.4 Cloud Storage

Version 2.4 adds configurable asset storage providers while keeping asset records portable.

- Local, Azure Blob, and S3 storage providers are available behind the same `IFileStorage` abstraction
- `Storage:Provider` controls the active provider
- Asset rows store relative `StoragePath` values, not absolute public URLs
- Asset URLs are resolved at response time through provider-specific public base URLs
- Local storage remains the default provider for development and integration tests

## Version 2.5 PostgreSQL Full-Text Search

Version 2.5 upgrades search from basic text matching to PostgreSQL full-text search.

- Added stored `tsvector` search columns for posts and assets
- Added GIN indexes for post and asset search vectors
- Search endpoints now use `websearch_to_tsquery` matching
- Search results rank by relevance before falling back to newest content
- Existing pagination and filters remain in place for post and asset search

## Version 2.6 Production Docker Compose

Version 2.6 adds a production-style Docker Compose stack without adding Nginx yet.

- Added `docker-compose.production.yml`
- Added API, PostgreSQL, Redis, and Mailpit services
- Added service health checks for PostgreSQL and Redis
- Added production environment wiring for database, Redis, JWT, auth links, background jobs, and storage
- Added SMTP email delivery wiring for Mailpit and real SMTP providers
- Added Redis-backed distributed locks for background jobs when multiple API instances are used
- Added startup switches for automatic database migrations and seed data
- Added fail-closed login throttling support for production Redis outages
- Updated the API Dockerfile so Docker builds restore all project references reliably

## Tech Stack

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core 10
- PostgreSQL 16
- Redis 7
- Npgsql
- StackExchange.Redis
- Azure.Storage.Blobs
- AWSSDK.S3
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
ContentHub.Infrastructure/       Authentication, storage, caching, rate limiting, and infrastructure services
tests/ContentHub.UnitTests/      Unit tests
tests/ContentHub.IntegrationTests/ Integration/API tests with Testcontainers PostgreSQL
tests/ContentHub.ArchitectureTests/ Architecture and dependency rule tests
```

## Prerequisites

- .NET 10 SDK
- Docker Desktop, required for PostgreSQL, Redis, and integration tests
- Redis is started by Docker Compose for caching, login throttling, and distributed background job locks

## Getting Started

Restore tools and packages:

```powershell
dotnet tool restore
dotnet restore
```

Start PostgreSQL and Redis:

```powershell
docker compose up -d
```

Start only Redis:

```powershell
docker compose up -d contenthub-redis
```

Explicitly enabled production seed data should be set as :

```powershell
$env:CONTENTHUB_SEED_ON_STARTUP = "true"
$env:CONTENTHUB_SEED_ADMIN_PASSWORD = "change-this-admin-password"
```

Production-style service URLs:

- API: `http://localhost:8080`
- Mailpit UI: `http://localhost:8025`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`

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

Default development Redis connection:

```text
localhost:6379
```

Default development storage provider:

```text
Storage:Provider = Local
Storage:RootPath = storage/assets
Storage:LocalRootPath = storage/assets
Storage:PublicBaseUrl = /assets
```

Cloud storage can be enabled by setting `Storage:Provider` to `AzureBlob` or `S3` and filling the matching `Storage:AzureBlob` or `Storage:S3` section.

SMTP email delivery can be enabled with:

```text
Email:Smtp:Enabled = true
Email:Smtp:Host = localhost
Email:Smtp:Port = 1025
```

The production compose stack points SMTP to Mailpit by default.

Automatic migrations and seed data can be controlled with:

```text
Database:ApplyMigrationsOnStartup = true
Database:SeedOnStartup = false
```

Development overrides enable seed data by default.

Development seeded admin user:

```text
Email: admin@contenthub.local
Username: admin
Password: configured with Seed:Admin:Password
```

## Database

The API can apply EF Core migrations at startup. In development it can also seed baseline roles, sample content, and the configured development admin user.

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

Integration tests require Docker because they use Testcontainers PostgreSQL.

Current v2.6 local verification:

```text
dotnet build --no-restore
Unit tests: 46/46 passed
Architecture tests: 22/22 passed
Integration tests: 40/40 passed
```

Production compose verification:

```text
docker compose -f docker-compose.production.yml config --quiet
```

The production compose verification requires the required production environment variables, such as `CONTENTHUB_POSTGRES_PASSWORD` and `CONTENTHUB_JWT_SECRET`, to be set.

## API Areas

- `/api/auth/*`
- `/api/categories`
- `/api/authors`
- `/api/posts`
- `/api/public/posts`
- `/api/public/featured-posts`
- `/api/assets`
- `/api/search`
- `/api/notifications`
- `/api/admin/audit-logs`
- `/health`, `/health/live`, `/health/ready`

## Notes

- Authorization is currently role-policy based. Permission constants exist, but full permission-based policy enforcement is not yet part of the current baseline.
- Asset storage supports local filesystem, Azure Blob, and S3 providers. The database stores provider and relative storage path, while URLs are resolved in API responses.
- Search uses PostgreSQL full-text search with stored vectors, GIN indexes, and relevance ranking. A dedicated search engine is still not part of the current baseline.
- Notifications are basic in-app notifications with hosted delivery processing. Auth email delivery supports SMTP and can be tested through Mailpit in the production compose stack.
- Background jobs publish scheduled posts, mark notification deliveries, and clean up old inactive auth tokens and sessions. They use Redis-backed distributed locks when `BackgroundJobs:RequireDistributedLock` is enabled.
- GET endpoints use request bodies when a query object exists. This is a deliberate project convention, but client/proxy compatibility should be checked before public API exposure.
- EF Core currently logs warnings for required relationships where the required entity has a global query filter. Tests pass, but this is worth revisiting as the data model evolves.
- FluentAssertions v8 prints a license notice during test runs.

## License

No license has been added yet.
