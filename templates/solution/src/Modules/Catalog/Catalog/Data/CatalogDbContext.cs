using Catalog.Categories.Models;
using Catalog.Products.Models;

namespace Catalog.Data;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : ModuleDbContext(options)
{
    public const string SchemaName = "catalog";

    public override string Schema => SchemaName;

    public DbSet<CatalogCategory> Categories => Set<CatalogCategory>();

    public DbSet<Product> Products => Set<Product>();
}
