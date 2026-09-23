namespace Shared.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Apply pending EF Core migrations of every module at startup. Prefer migration bundles in production.</summary>
    public bool MigrateOnStartup { get; set; }

    /// <summary>Run every registered <see cref="IDataSeeder"/> after migrating.</summary>
    public bool SeedOnStartup { get; set; }
}
