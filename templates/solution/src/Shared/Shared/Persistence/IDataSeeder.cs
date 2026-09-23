namespace Shared.Persistence;

/// <summary>Seeds (development) data for a module. Implementations must be idempotent.</summary>
public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}
