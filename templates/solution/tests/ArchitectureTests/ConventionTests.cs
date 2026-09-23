using System.Reflection;
using Shared.Endpoints;

namespace ArchitectureTests;

public sealed class ConventionTests
{
    private static IEnumerable<Type> ModuleTypes =>
        Modules.ImplementationAssemblies.SelectMany(assembly => assembly.GetTypes());

    [Fact]
    public void Mappers_are_static_classes()
    {
        var offenders = ModuleTypes
            .Where(type => type.Name.EndsWith("Mapper", StringComparison.Ordinal))
            .Where(type => !(type.IsAbstract && type.IsSealed))
            .Select(type => type.FullName)
            .ToList();

        offenders.ShouldBeEmpty("Mappers are plain static classes, e.g. CreateCategoryMapper.ToEntity(command).");
    }

    [Fact]
    public void Endpoints_are_internal_and_sealed()
    {
        var offenders = ModuleTypes
            .Where(type => type.IsAssignableTo(typeof(IEndpoint)) && !type.IsInterface)
            .Where(type => type.IsPublic || !type.IsSealed)
            .Select(type => type.FullName)
            .ToList();

        offenders.ShouldBeEmpty();
    }

    [Fact]
    public void Message_handlers_are_public_so_Wolverine_can_generate_code_for_them()
    {
        var offenders = ModuleTypes
            .Where(type => type.Name.EndsWith("Handler", StringComparison.Ordinal) && !type.IsNested)
            .Where(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Any(method => method.Name is "Handle" or "HandleAsync"))
            .Where(type => !type.IsPublic)
            .Select(type => type.FullName)
            .ToList();

        offenders.ShouldBeEmpty();
    }

    [Fact]
    public void Every_module_has_a_route_prefix_and_its_own_assembly()
    {
        foreach (var module in Modules.All)
        {
            module.RoutePrefix.ShouldNotBeNullOrWhiteSpace();
            module.Assembly.ShouldNotBe(typeof(IEndpoint).Assembly);
        }
    }
}
