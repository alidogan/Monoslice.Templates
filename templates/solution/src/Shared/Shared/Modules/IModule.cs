using System.Reflection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace Shared.Modules;

/// <summary>
/// Entry point of a module. The host registers modules explicitly (see <c>ModuleRegistry</c> in the Api project).
/// </summary>
public interface IModule
{
    /// <summary>Display name, also used as OpenAPI tag.</summary>
    string Name { get; }

    /// <summary>Route segment: endpoints are mapped under <c>/api/{RoutePrefix}</c>.</summary>
    string RoutePrefix => Name.ToLowerInvariant();

    /// <summary>Assembly that contains the module's handlers and endpoints.</summary>
    Assembly Assembly => GetType().Assembly;

    /// <summary>Registers the module's services, e.g. its DbContext.</summary>
    void AddServices(IHostApplicationBuilder builder);

    /// <summary>Optional module specific messaging configuration, e.g. extra listeners or routing rules.</summary>
    void ConfigureMessaging(WolverineOptions options)
    {
    }
}
