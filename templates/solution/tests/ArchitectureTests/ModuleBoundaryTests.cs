using NetArchTest.Rules;

namespace ArchitectureTests;

public sealed class ModuleBoundaryTests
{
    private static readonly string[] InfrastructureNamespaces =
    [
        "Microsoft.EntityFrameworkCore",
        "Wolverine",
        "Microsoft.AspNetCore",
    ];

    [Fact]
    public void Modules_only_reference_the_contracts_of_other_modules()
    {
        var moduleAssemblyNames = Modules.ImplementationAssemblies
            .Select(assembly => assembly.GetName().Name!)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var assembly in Modules.ImplementationAssemblies)
        {
            var forbidden = assembly.GetReferencedAssemblies()
                .Select(reference => reference.Name!)
                .Where(moduleAssemblyNames.Contains)
                .ToList();

            forbidden.ShouldBeEmpty(
                $"{assembly.GetName().Name} references other modules directly. Reference their Contracts instead.");
        }
    }

    [Fact]
    public void Contracts_do_not_depend_on_infrastructure()
    {
        foreach (var contracts in Modules.ContractsAssemblies)
        {
            var result = Types.InAssembly(contracts)
                .ShouldNot()
                .HaveDependencyOnAny(InfrastructureNamespaces)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue(
                $"Contracts must stay dependency free, but these types are not: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void Domain_models_do_not_depend_on_infrastructure()
    {
        foreach (var assembly in Modules.ImplementationAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceMatching(@"\.Models$")
                .ShouldNot()
                .HaveDependencyOnAny(InfrastructureNamespaces)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue(
                $"Domain models must not depend on infrastructure: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void No_mapping_or_commercial_messaging_libraries_are_referenced()
    {
        string[] banned = ["AutoMapper", "Mapster", "MediatR", "MassTransit"];

        var offenders = Modules.ImplementationAssemblies
            .Concat(Modules.ContractsAssemblies)
            .Append(typeof(Shared.Modules.IModule).Assembly)
            .SelectMany(assembly => assembly.GetReferencedAssemblies())
            .Select(reference => reference.Name!)
            .Where(name => banned.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        offenders.ShouldBeEmpty();
    }
}
