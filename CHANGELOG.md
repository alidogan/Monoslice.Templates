# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [1.0.0]

### Added

- `ms-sln`: modular monolith solution on .NET 10 with a sample Catalog module.
  - Options: `--database postgresql|sqlserver`, `--broker none|rabbitmq|azureservicebus`, `--auth jwt|none`,
    `--include-sample`, `--docker`, `--ci github|azure|none`.
  - Wolverine as mediator and message bus with a transactional outbox and durable local queues.
  - EF Core with one schema per module, auditing, soft delete and optimistic concurrency (ETag / If-Match).
  - Result pattern with RFC 9457 problem details, rate limiting and security headers.
  - .NET Aspire AppHost, Serilog and OpenTelemetry.
  - Unit, integration (Testcontainers) and architecture tests.
- `ms-module`: adds a module with its Contracts and unit test projects.
- `ms-feature`: adds a command or query vertical slice to a module.
