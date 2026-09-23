using Catalog.Categories.Dtos;

namespace Catalog.Categories.Features.GetCategoryById;

internal sealed class GetCategoryByIdEndpoint : IEndpoint
{
    public const string Name = "Catalog.GetCategoryById";

    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/categories/{id:guid}", async (
                Guid id,
                HttpResponse response,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var result = await bus.InvokeAsync<Result<GetCategoryByIdResult>>(
                    GetCategoryByIdMapper.ToQuery(id),
                    cancellationToken);

                return result.ToHttpResult(found =>
                {
                    response.Headers.ETag = found.ETag;
                    return TypedResults.Ok(found.Category);
                });
            })
            .WithName(Name)
            .WithSummary("Gets a category. The ETag response header is its current version.")
            .Produces<CatalogCategoryDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
