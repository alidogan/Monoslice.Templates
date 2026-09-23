namespace Catalog.Data;

/// <summary>Used by <c>dotnet ef</c> to create migrations.</summary>
internal sealed class CatalogDbContextFactory : ModuleDbContextFactory<CatalogDbContext>
{
    protected override string Schema => CatalogDbContext.SchemaName;
}
