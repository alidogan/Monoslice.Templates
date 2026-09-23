namespace ModuleName.AggregateName.Features.FeatureName;

public sealed record FeatureNameQuery(Guid Id) : IQuery<Result<FeatureNameResult>>;

public sealed record FeatureNameResult(Guid Id);
