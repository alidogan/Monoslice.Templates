using Catalog.Categories.Features.GetCategoryById;

namespace Catalog.Categories.Features.CreateCategory;

public sealed record CreateCategoryRequest(string Name, string? Description, int DisplayOrder, bool IsActive);

public sealed record CreateCategoryResponse(Guid Id);

internal sealed class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/categories", async (CreateCategoryRequest request, IMessageBus bus, CancellationToken cancellationToken) =>
            {
                var command = CreateCategoryMapper.ToCommand(request);
                var result = await bus.InvokeAsync<Result<CreateCategoryResult>>(command, cancellationToken);

                return result.ToHttpResult(created => TypedResults.CreatedAtRoute(
                    CreateCategoryMapper.ToResponse(created),
                    GetCategoryByIdEndpoint.Name,
                    new { id = created.Id }));
            })
            .WithName("Catalog.CreateCategory")
            .WithSummary("Creates a category")
            .Produces<CreateCategoryResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
}
