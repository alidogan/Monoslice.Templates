using Catalog.Products.Dtos;

namespace Catalog.Products.Features.GetProducts;

internal sealed class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/products", async (
                [AsParameters] PagedRequest paging,
                Guid? categoryId,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var result = await bus.InvokeAsync<Result<PagedResult<ProductDto>>>(
                    GetProductsMapper.ToQuery(paging, categoryId),
                    cancellationToken);

                return result.ToHttpResult();
            })
            .WithName("Catalog.GetProducts")
            .WithSummary("Lists products, optionally filtered by category")
            .Produces<PagedResult<ProductDto>>();
}
