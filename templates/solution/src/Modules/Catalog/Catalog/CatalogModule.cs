using Catalog.Data.Seed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Modules;

namespace Catalog;

public sealed class CatalogModule : IModule
{
    public string Name => "Catalog";

    public void AddServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<CatalogDbContext>(CatalogDbContext.SchemaName);
        builder.Services.AddScoped<IDataSeeder, CatalogSeeder>();
    }
}
