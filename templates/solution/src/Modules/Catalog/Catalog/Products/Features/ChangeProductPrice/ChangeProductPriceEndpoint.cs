namespace Catalog.Products.Features.ChangeProductPrice;

public sealed record ChangeProductPriceRequest(decimal Price);

internal sealed class ChangeProductPriceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("/products/{id:guid}/price", async (
                Guid id,
                ChangeProductPriceRequest request,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                var result = await bus.InvokeAsync<Result>(
                    ChangeProductPriceMapper.ToCommand(id, request),
                    cancellationToken);

                return result.ToHttpResult(TypedResults.NoContent);
            })
            .WithName("Catalog.ChangeProductPrice")
            .WithSummary("Changes a product's price and publishes ProductPriceChangedIntegrationEvent")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
