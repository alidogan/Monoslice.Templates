using JasperFx.RuntimeCompiler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Exceptions;
using Shared.Messaging;
using Shared.Modules;
using Shared.Persistence;
using Shared.Security;

namespace Shared.Hosting;

public static class ModuleHostExtensions
{
    public const string CacheConnectionStringName = "cache";

    /// <summary>Registers the shared infrastructure, every module and Wolverine.</summary>
    public static IHostApplicationBuilder AddModules(
        this IHostApplicationBuilder builder,
        IReadOnlyCollection<IModule> modules)
    {
        var services = builder.Services;

        services.AddHttpContextAccessor();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<ICurrentUser, CurrentUser>();
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddSingleton<SoftDeleteInterceptor>();

        services.AddOptions<DatabaseOptions>().BindConfiguration(DatabaseOptions.SectionName);
        services.AddHostedService<DatabaseInitializer>();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // HybridCache: in-memory, plus Redis as second level when a "cache" connection string is configured.
        if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString(CacheConnectionStringName)))
        {
            builder.AddRedisDistributedCache(CacheConnectionStringName);
        }

        services.AddHybridCache();

        foreach (var module in modules)
        {
            module.AddServices(builder);
        }

        // Compiles Wolverine's generated handler code at runtime when no pre-generated code is available.
        services.AddRuntimeCompilation();
        builder.UseModularWolverine(modules);
        return builder;
    }
}
