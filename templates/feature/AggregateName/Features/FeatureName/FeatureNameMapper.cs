namespace ModuleName.AggregateName.Features.FeatureName;

public static class FeatureNameMapper
{
#if (IsCommand)
    public static FeatureNameCommand ToCommand(FeatureNameRequest request) => new(request.Name);

    // public static Order ToEntity(FeatureNameCommand command) =>
    //     Order.Create(Guid.CreateVersion7(), command.Name);
    //
    // public static FeatureNameResult ToResult(Order order) => new(order.Id);
#else
    public static FeatureNameQuery ToQuery(Guid id) => new(id);

    // public static readonly Expression<Func<Order, FeatureNameResult>> Projection = order =>
    //     new FeatureNameResult(order.Id);
#endif

    public static FeatureNameResponse ToResponse(FeatureNameResult result) => new(result.Id);
}
