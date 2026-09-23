namespace Shared.Domain;

/// <summary>
/// Optimistic concurrency token, mapped by <c>ModuleDbContext</c> to the database row version
/// (<c>xmin</c> on PostgreSQL, <c>rowversion</c> on SQL Server).
/// Use <c>ConcurrencyToken</c> to turn it into an HTTP ETag and back.
/// </summary>
public interface IHasConcurrencyToken
{
#if (UsePostgreSQL)
    uint Version { get; }
#else
    byte[] Version { get; }
#endif
}
