namespace ModuleName.Data;

public sealed class ModuleNameDbContext(DbContextOptions<ModuleNameDbContext> options) : ModuleDbContext(options)
{
    public const string SchemaName = "modulename";

    public override string Schema => SchemaName;

    // Add a DbSet per aggregate root, e.g.:
    // public DbSet<Order> Orders => Set<Order>();
}
