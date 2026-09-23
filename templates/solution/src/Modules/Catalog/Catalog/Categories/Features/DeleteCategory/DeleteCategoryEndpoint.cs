using Catalog.Categories.Features.UpdateCategory;

namespace Catalog.Categories.Features.DeleteCategory;

internal sealed class DeleteCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapDelete("/categories/{id:guid}", async (
                Guid id,
                [FromHeader(Name = "If-Match")] string? ifMatch,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(ifMatch))
                {
                    return UpdateCategoryEndpoint.PreconditionRequired();
                }

                var result = await bus.InvokeAsync<Result>(DeleteCategoryMapper.ToCommand(id, ifMatch), cancellationToken);

                return result.ToHttpResult(TypedResults.NoContent);
            })
            .WithName("Catalog.DeleteCategory")
            .WithSummary("Soft deletes a category. Requires the category's ETag in the If-Match header.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status412PreconditionFailed)
            .ProducesProblem(StatusCodes.Status428PreconditionRequired);
}
