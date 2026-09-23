namespace ModuleName.AggregateName.Features.FeatureName;

public sealed record FeatureNameCommand(string Name) : ICommand<Result<FeatureNameResult>>;

public sealed record FeatureNameResult(Guid Id);

public sealed class FeatureNameCommandValidator : AbstractValidator<FeatureNameCommand>
{
    public FeatureNameCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
    }
}
