using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shared.Persistence;

/// <summary>
/// Applies every module's migrations (and optionally seeds data) before the application starts handling traffic.
/// Controlled by the <c>Database</c> configuration section.
/// </summary>
internal sealed partial class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    IEnumerable<ModuleDbContextRegistration> registrations,
    IOptions<DatabaseOptions> options,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.MigrateOnStartup)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();

        foreach (var registration in registrations)
        {
            var dbContext = (DbContext)scope.ServiceProvider.GetRequiredService(registration.DbContextType);
            LogMigrating(logger, registration.DbContextType.Name);
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (!options.Value.SeedOnStartup)
        {
            return;
        }

        foreach (var seeder in scope.ServiceProvider.GetServices<IDataSeeder>())
        {
            await seeder.SeedAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying migrations for {DbContext}")]
    private static partial void LogMigrating(ILogger logger, string dbContext);
}
