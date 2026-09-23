namespace ModuleName.Data;

/// <summary>Used by <c>dotnet ef</c> to create migrations.</summary>
internal sealed class ModuleNameDbContextFactory : ModuleDbContextFactory<ModuleNameDbContext>
{
    protected override string Schema => ModuleNameDbContext.SchemaName;
}
