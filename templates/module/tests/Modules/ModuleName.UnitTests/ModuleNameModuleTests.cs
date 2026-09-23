using Shared.Modules;

namespace ModuleName.UnitTests;

public sealed class ModuleNameModuleTests
{
    [Fact]
    public void Module_is_mapped_under_its_own_route_prefix()
    {
        IModule module = new ModuleNameModule();

        module.RoutePrefix.ShouldBe("modulename");
        module.Assembly.ShouldBe(typeof(ModuleNameModule).Assembly);
    }
}
