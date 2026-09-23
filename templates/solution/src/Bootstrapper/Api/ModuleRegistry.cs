#if (IncludeSample)
using Catalog;
#endif
using Shared.Modules;

namespace Api;

/// <summary>
/// The modules hosted by this application. Register a new module here after creating it with
/// <c>dotnet new ms-module</c>.
/// </summary>
internal static class ModuleRegistry
{
    public static IReadOnlyList<IModule> Modules { get; } =
    [
#if (IncludeSample)
        new CatalogModule(),
#endif
    ];
}
