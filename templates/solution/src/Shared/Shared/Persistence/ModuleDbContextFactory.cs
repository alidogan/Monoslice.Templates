using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shared.Persistence;

/// <summary>
/// Base for a module's design-time factory, used by <c>dotnet ef</c> to create migrations without starting the app.
/// </summary>
public abstract class ModuleDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : ModuleDbContext
{
    // Only used to pick the provider and SQL dialect; dotnet ef does not connect when adding migrations.
#if (UsePostgreSQL)
    private const string DesignTimeConnectionString = "Host=localhost;Database=design;Username=design;Password=design";
#else
    private const string DesignTimeConnectionString = "Server=localhost;Database=design;User Id=design;Password=design;TrustServerCertificate=true";
#endif

    protected abstract string Schema { get; }

    public TContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TContext>();
        DatabaseProvider.Configure(options, DesignTimeConnectionString, Schema);
        return (TContext)Activator.CreateInstance(typeof(TContext), options.Options)!;
    }
}
