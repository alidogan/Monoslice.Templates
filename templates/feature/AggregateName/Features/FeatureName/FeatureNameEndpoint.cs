namespace ModuleName.AggregateName.Features.FeatureName;

#if (IsCommand)
public sealed record FeatureNameRequest(string Name);

public sealed record FeatureNameResponse(Guid Id);

internal sealed class FeatureNameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/aggregateroute", async (FeatureNameRequest request, IMessageBus bus, CancellationToken cancellationToken) =>
            {
                var command = FeatureNameMapper.ToCommand(request);
                var result = await bus.InvokeAsync<Result<FeatureNameResult>>(command, cancellationToken);

                return result.ToHttpResult(value => TypedResults.Ok(FeatureNameMapper.ToResponse(value)));
            })
            .WithName("ModuleName.FeatureName")
            .WithSummary("FeatureName")
            .Produces<FeatureNameResponse>()
            .ProducesValidationProblem();
}
#else
public sealed record FeatureNameResponse(Guid Id);

internal sealed class FeatureNameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/aggregateroute/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken cancellationToken) =>
            {
                var query = FeatureNameMapper.ToQuery(id);
                var result = await bus.InvokeAsync<Result<FeatureNameResult>>(query, cancellationToken);

                return result.ToHttpResult(value => TypedResults.Ok(FeatureNameMapper.ToResponse(value)));
            })
            .WithName("ModuleName.FeatureName")
            .WithSummary("FeatureName")
            .Produces<FeatureNameResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
#endif
