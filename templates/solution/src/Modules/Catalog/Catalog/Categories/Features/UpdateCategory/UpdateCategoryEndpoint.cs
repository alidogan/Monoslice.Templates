namespace Catalog.Categories.Features.UpdateCategory;

public sealed record UpdateCategoryRequest(string Name, string? Description, int DisplayOrder, bool IsActive);

internal sealed class UpdateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("/categories/{id:guid}", async (
                Guid id,
                UpdateCategoryRequest request,
                [FromHeader(Name = "If-Match")] string? ifMatch,
                IMessageBus bus,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(ifMatch))
                {
                    return PreconditionRequired();
                }

                var command = UpdateCategoryMapper.ToCommand(id, request, ifMatch);
                var result = await bus.InvokeAsync<Result>(command, cancellationToken);

                return result.ToHttpResult(TypedResults.NoContent);
            })
            .WithName("Catalog.UpdateCategory")
            .WithSummary("Updates a category. Requires the category's ETag in the If-Match header.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status412PreconditionFailed)
            .ProducesProblem(StatusCodes.Status428PreconditionRequired);

    internal static IResult PreconditionRequired() =>
        TypedResults.Problem(
            statusCode: StatusCodes.Status428PreconditionRequired,
            title: "Precondition Required",
            detail: "Send the resource's ETag in the If-Match header.");
}
