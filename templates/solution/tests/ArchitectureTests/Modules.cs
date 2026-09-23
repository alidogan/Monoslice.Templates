using System.Reflection;
using Api;
using Shared.Modules;

namespace ArchitectureTests;

/// <summary>The modules registered in the Api, with their implementation and Contracts assemblies.</summary>
internal static class Modules
{
    public static IReadOnlyList<IModule> All => ModuleRegistry.Modules;

    public static IEnumerable<Assembly> ImplementationAssemblies => All.Select(module => module.Assembly);

    public static IEnumerable<Assembly> ContractsAssemblies =>
        All.Select(module => module.Assembly.GetReferencedAssemblies()
                .FirstOrDefault(reference => string.Equals(
                    reference.Name,
                    $"{module.Assembly.GetName().Name}.Contracts",
                    StringComparison.Ordinal)))
            .Where(reference => reference is not null)
            .Select(reference => Assembly.Load(reference!));
}
