# Monoslice.Templates

**Production-ready modular monolith templates for .NET 10.**

Monoslice scaffolds a modular monolith with vertical slices, explicit mappers and
[Wolverine](https://wolverinefx.net) as mediator and message bus. There are no commercial dependencies: no MediatR,
AutoMapper, Mapster or MassTransit.

```bash
dotnet new install Monoslice.Templates
dotnet new ms-sln -n Acme.Shop
cd Acme.Shop
dotnet run --project src/AppHost
```

## What you get

- **Modules with hard boundaries.** Each module has its own projects, its own database schema and a `*.Contracts`
  project as its only public API. Architecture tests enforce this.
- **Vertical slices.** Each feature folder holds its endpoint, command or query, validator, handler and mapper.
- **Explicit mapping.** Every feature has a static `{Feature}Mapper` class, so there is no mapping library and no
  runtime magic:

  ```csharp
  public static class CreateCategoryMapper
  {
      public static CatalogCategory ToEntity(CreateCategoryCommand command) =>
          CatalogCategory.Create(Guid.CreateVersion7(), command.Name, command.Description, command.DisplayOrder, command.IsActive);

      public static CreateCategoryResult ToResult(CatalogCategory category) => new(category.Id);
  }
  ```

- **Wolverine** handles commands, queries and events.
  - Handlers never call `SaveChangesAsync`. Wolverine commits the EF Core changes, the domain events and the
    transactional outbox in one transaction.
  - Durable local queues deliver events between modules.
  - You can switch to RabbitMQ or Azure Service Bus with one option. The handlers stay the same.
- **EF Core** stores data with one schema per module. It also provides:
  - auditing
  - soft delete
  - optimistic concurrency, exposed as `ETag` and `If-Match` headers
- **Result pattern.** Expected failures are `Error` values that become RFC 9457 problem details. Exceptions are only
  used for unexpected failures.
- **Production concerns.** The generated API includes:
  - JWT bearer authentication for any OpenID Connect provider (Keycloak runs locally)
  - rate limiting and security headers
  - Serilog, OpenTelemetry and health checks
  - a Dockerfile that pre-generates Wolverine's handler code
- **.NET Aspire** orchestrates local development: the database, Redis, Keycloak and the broker.
- **Tests.** The solution comes with three kinds of tests:
  - unit tests
  - integration tests that call the real API over HTTP against a real database, using Testcontainers and Respawn
  - architecture tests (NetArchTest)
- **Quality gates.** Central package management, analyzers (Meziantou, Roslynator), warnings as errors,
  `dotnet format` in CI and a GitHub Actions or Azure Pipelines pipeline.

## Templates

| Template | Command | Run from |
|---|---|---|
| Solution | `dotnet new ms-sln -n Acme.Shop` | anywhere |
| Module | `dotnet new ms-module -n Ordering` | the solution root |
| Feature | `dotnet new ms-feature -n CreateOrder --aggregate Orders --kind command` | a module project folder |

### `ms-sln` options

| Option | Values (default in bold) | Description |
|---|---|---|
| `--database`, `-db` | **`postgresql`**, `sqlserver` | Database for all modules (one schema per module) |
| `--broker`, `-b` | **`none`**, `rabbitmq`, `azureservicebus` | Where integration events travel. `none` keeps messaging in-process with durable local queues and an outbox |
| `--auth` | **`jwt`**, `none` | JWT bearer authentication (Keycloak for local development) |
| `--include-sample` | **`true`**, `false` | Include the sample Catalog module and its tests |
| `--docker` | **`true`**, `false` | Add a Dockerfile and `docker-compose.yml` |
| `--ci` | **`github`**, `azure`, `none` | Add a CI pipeline |

Example:

```bash
dotnet new ms-sln -n Acme.Shop --database sqlserver --broker rabbitmq
```

## Generated solution

```
Acme.Shop.slnx
src/
  AppHost/                      .NET Aspire orchestration
  ServiceDefaults/              OpenTelemetry, Serilog, health checks, resilience
  Bootstrapper/Api/             Host: module registry, auth, rate limiting, OpenAPI (Scalar)
  Shared/Shared.Contracts/      ICommand/IQuery markers, Result/Error, paging, IntegrationEvent (no dependencies)
  Shared/Shared/                DDD base classes, EF Core and Wolverine setup, endpoint discovery, problem details
  Modules/Catalog/Catalog/            Sample module: categories and products
  Modules/Catalog/Catalog.Contracts/  Its public API: a contract query and an integration event
tests/
  ArchitectureTests/  IntegrationTests/  Modules/Catalog.UnitTests/
```

A feature slice looks like this:

```
Catalog/Categories/Features/CreateCategory/
  CreateCategoryEndpoint.cs   // request/response + IEndpoint (Minimal API) -> bus.InvokeAsync(...)
  CreateCategoryCommand.cs    // command, result and FluentValidation validator
  CreateCategoryHandler.cs    // static Wolverine handler returning Result<T>
  CreateCategoryMapper.cs     // request -> command -> entity -> result -> response
```

## Why Wolverine?

- **Mediator and messaging in one MIT-licensed library.** In-process `InvokeAsync`, durable local queues, a
  transactional outbox and inbox on PostgreSQL or SQL Server, and transports for RabbitMQ, Azure Service Bus and others.
- **Domain events are published only after the transaction commits,** through the outbox.
- **Handlers are plain static methods,** with no interfaces to implement.

## Requirements

- .NET 10 SDK
- Docker, for .NET Aspire, the integration tests and docker compose

## Development

See [CONTRIBUTING.md](CONTRIBUTING.md). In short:

```bash
./scripts/test-templates.sh          # pack, install, generate and test the main variants
./scripts/regenerate-migrations.sh   # regenerate the sample migrations for both databases
```

## License

[MIT](LICENSE)
