using Microsoft.AspNetCore.Routing;

namespace Shared.Endpoints;

/// <summary>
/// A Minimal API endpoint. Implementations are discovered per module and mapped onto the module's
/// route group (<c>/api/{module}</c>), so routes are relative to that group.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
