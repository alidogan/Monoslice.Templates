using Catalog.Categories.Dtos;

namespace Catalog.Categories.Features.GetCategories;

internal sealed class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/categories", async (
                [AsParameters] PagedRequest paging,
                string? search,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var result = await bus.InvokeAsync<Result<PagedResult<CatalogCategoryDto>>>(
                    GetCategoriesMapper.ToQuery(paging, search),
                    cancellationToken);

                return result.ToHttpResult();
            })
            .WithName("Catalog.GetCategories")
            .WithSummary("Lists categories, ordered by display order")
            .Produces<PagedResult<CatalogCategoryDto>>();
}
