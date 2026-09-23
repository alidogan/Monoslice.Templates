namespace Catalog.Products.Features.CreateProduct;

public sealed record CreateProductRequest(Guid CategoryId, string Name, string? Description, decimal Price);

public sealed record CreateProductResponse(Guid Id);

internal sealed class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/products", async (CreateProductRequest request, IMessageBus bus, CancellationToken cancellationToken) =>
            {
                var command = CreateProductMapper.ToCommand(request);
                var result = await bus.InvokeAsync<Result<CreateProductResult>>(command, cancellationToken);

                return result.ToHttpResult(created => TypedResults.Created(
                    $"/api/catalog/products/{created.Id}",
                    CreateProductMapper.ToResponse(created)));
            })
            .WithName("Catalog.CreateProduct")
            .WithSummary("Creates a product in an existing category")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
}
