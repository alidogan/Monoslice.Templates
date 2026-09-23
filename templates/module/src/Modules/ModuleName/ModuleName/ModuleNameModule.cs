using Microsoft.Extensions.Hosting;
using Shared.Modules;

namespace ModuleName;

public sealed class ModuleNameModule : IModule
{
    public string Name => "ModuleName";

    public void AddServices(IHostApplicationBuilder builder) =>
        builder.AddModuleDbContext<ModuleNameDbContext>(ModuleNameDbContext.SchemaName);
}
