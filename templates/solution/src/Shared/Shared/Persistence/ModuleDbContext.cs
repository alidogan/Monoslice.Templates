using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace Shared.Persistence;

/// <summary>
/// Base class for a module's DbContext. Every module owns exactly one schema inside the shared database,
/// which keeps modules isolated and extractable.
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options) : DbContext(options)
{
    public abstract string Schema { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplySharedConventions();

        // Lets Wolverine store outgoing messages in the same transaction as the module's changes (outbox).
        // The tables are owned by Wolverine, so they are excluded from the module's migrations.
        modelBuilder.MapWolverineEnvelopeStorage(DatabaseDefaults.MessagingSchema);
        modelBuilder.ExcludeSchemaFromMigrations(DatabaseDefaults.MessagingSchema);

        base.OnModelCreating(modelBuilder);
    }
}
