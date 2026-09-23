# ModularMonolith

A modular monolith generated with [Monoslice](https://www.nuget.org/packages/Monoslice.Templates): .NET 10, vertical slices,
Wolverine, EF Core and .NET Aspire.

## Getting started

Requirements: the .NET 10 SDK and Docker (for Aspire, Testcontainers and docker compose).

```bash
dotnet run --project src/AppHost
```

The Aspire dashboard opens with the API and its dependencies. The API reference (Scalar) is at `/scalar` on the
API's address. In Development, migrations run and sample data is seeded on startup.
<!--#if (UseAuth)-->

### Getting a token

Keycloak runs on http://localhost:8080 (admin / admin) with realm `app`, client `app-dev` and user `alice` / `alice`:

```bash
curl -s -X POST http://localhost:8080/realms/app/protocol/openid-connect/token \
  -d grant_type=password -d client_id=app-dev -d client_secret=app-dev-secret \
  -d username=alice -d password=alice
```

Send the `access_token` as `Authorization: Bearer <token>`. `src/Bootstrapper/Api/Api.http` has ready-made requests.
<!--#endif-->
<!--#if (UseDocker)-->

### Without Aspire

```bash
docker compose up --build
```

The API listens on http://localhost:8081 and logs to Seq (http://localhost:5341).
<!--#endif-->

## Tests

```bash
dotnet test
```

- `tests/Modules/*.UnitTests`: domain, mappers and validators.
- `tests/IntegrationTests`: the real API over HTTP against a database in a container (Testcontainers), reset with Respawn.
- `tests/ArchitectureTests`: module boundaries and conventions.

## Architecture

```
src/
  AppHost/                  .NET Aspire orchestration (database, cache, identity provider, broker)
  ServiceDefaults/          OpenTelemetry, Serilog, health checks, resilience
  Bootstrapper/Api/         The host: registers modules, authentication, rate limiting, OpenAPI
  Shared/Shared.Contracts/  Dependency-free: ICommand / IQuery markers, Result, Error, paging, IntegrationEvent
  Shared/Shared/            Building blocks: DDD base classes, EF Core, Wolverine setup, endpoints, problem details
  Modules/<Module>/<Module>            The module's implementation
  Modules/<Module>/<Module>.Contracts  The module's public API: DTOs, queries and integration events
```

- **Modules** own a database schema and talk to each other only through `*.Contracts`: synchronously with
  `IMessageBus.InvokeAsync` on a contract query, asynchronously with integration events.
- **Vertical slices**: a feature folder holds its endpoint, command/query, validator, handler and mapper.
- **Mapping** is explicit: a static `{Feature}Mapper` class per feature, no mapping library.
- **Wolverine** is the mediator and message bus. Handlers don't call `SaveChangesAsync`: Wolverine wraps each
  command in a transaction and commits the changes, the domain events and the outbox together.
  Query handlers are marked `[NonTransactional]`.
- **Results**: expected failures are `Result`/`Error` values that become RFC 9457 problem details;
  exceptions are for unexpected failures.
- **Optimistic concurrency**: GET returns an `ETag`; PUT/DELETE require `If-Match` (412 when stale, 428 when missing).
- **Soft delete**: entities implementing `ISoftDeletable` are marked deleted and filtered from queries.
<!--#if (UseBroker)-->
- **Broker**: integration events travel over the message broker, so a module can later move to its own service.
  Domain events and commands stay in-process.
<!--#else-->
- **Messaging** is in-process with durable local queues and a transactional outbox. To add RabbitMQ or Azure Service Bus
  later, add `WolverineFx.RabbitMQ` (or `WolverineFx.AzureServiceBus`) and configure it in
  `src/Shared/Shared/Messaging/MessagingExtensions.cs`; handlers do not change.
<!--#endif-->

## Adding a module

```bash
dotnet new ms-module -n Ordering
```

Then register `new OrderingModule()` in `src/Bootstrapper/Api/ModuleRegistry.cs` and add its first migration:

```bash
dotnet tool restore
dotnet ef migrations add InitialCreate -p src/Modules/Ordering/Ordering -s src/Bootstrapper/Api -o Data/Migrations
```

## Adding a feature

From inside a module project:

```bash
cd src/Modules/Ordering/Ordering
dotnet new ms-feature -n CreateOrder --aggregate Orders --kind command
dotnet new ms-feature -n GetOrders --aggregate Orders --kind query
```

## Production notes

- Set `Database:MigrateOnStartup` to `false` (the default outside Development) and apply migrations with
  [EF Core migration bundles](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying#bundles).
- The Dockerfile pre-generates Wolverine's handler code (`dotnet run -- codegen write`) so containers start fast.
- Configure `Cors:AllowedOrigins`, `RateLimiting:*`<!--#if (UseAuth)-->, `Authentication:*`<!--#endif--> and
  `ConnectionStrings:*` per environment.
