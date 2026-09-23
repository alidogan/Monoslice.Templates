using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Modules;

namespace Shared.Endpoints;

public static class EndpointExtensions
{
    /// <summary>Maps every <see cref="IEndpoint"/> of every module onto a <c>/api/{module}</c> route group.</summary>
    public static IEndpointRouteBuilder MapModuleEndpoints(
        this IEndpointRouteBuilder app,
        IEnumerable<IModule> modules,
        Action<RouteGroupBuilder>? configureGroup = null)
    {
        foreach (var module in modules)
        {
            var group = app.MapGroup($"/api/{module.RoutePrefix}").WithTags(module.Name);
            configureGroup?.Invoke(group);

            foreach (var endpoint in DiscoverEndpoints(module))
            {
                endpoint.MapEndpoint(group);
            }
        }

        return app;
    }

    private static IEnumerable<IEndpoint> DiscoverEndpoints(IModule module) =>
        module.Assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint)))
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .Select(type => (IEndpoint)Activator.CreateInstance(type, nonPublic: true)!);
}
