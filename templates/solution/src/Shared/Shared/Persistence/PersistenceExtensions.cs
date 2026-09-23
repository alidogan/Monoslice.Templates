using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine.EntityFrameworkCore;

namespace Shared.Persistence;

public static class PersistenceExtensions
{
    /// <summary>
    /// Registers a module DbContext against the shared database, integrated with Wolverine's
    /// transactional middleware and outbox, with auditing, soft delete and Aspire telemetry/health checks.
    /// </summary>
    public static IHostApplicationBuilder AddModuleDbContext<TContext>(this IHostApplicationBuilder builder, string schema)
        where TContext : ModuleDbContext
    {
        builder.Services.AddDbContextWithWolverineIntegration<TContext>(
            (services, options) =>
            {
                var connectionString = services.GetRequiredService<IConfiguration>()
                    .GetConnectionString(DatabaseDefaults.ConnectionStringName)
                    ?? throw new InvalidOperationException(
                        $"Connection string '{DatabaseDefaults.ConnectionStringName}' is not configured.");

                DatabaseProvider.Configure(options, connectionString, schema);
                options.AddInterceptors(
                    services.GetRequiredService<SoftDeleteInterceptor>(),
                    services.GetRequiredService<AuditableEntityInterceptor>());
            },
            DatabaseDefaults.MessagingSchema);

        // Retries are disabled: Wolverine owns the transaction, and retrying inside it is not safe.
#if (UsePostgreSQL)
        builder.EnrichNpgsqlDbContext<TContext>(settings => settings.DisableRetry = true);
#else
        builder.EnrichSqlServerDbContext<TContext>(settings => settings.DisableRetry = true);
#endif

        builder.Services.AddSingleton(new ModuleDbContextRegistration(typeof(TContext)));
        return builder;
    }
}

/// <summary>Remembers which DbContexts belong to modules, so they can be migrated at startup.</summary>
public sealed record ModuleDbContextRegistration(Type DbContextType);
