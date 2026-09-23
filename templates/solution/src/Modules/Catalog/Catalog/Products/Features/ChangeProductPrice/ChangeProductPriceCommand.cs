namespace Catalog.Products.Features.ChangeProductPrice;

public sealed record ChangeProductPriceCommand(Guid ProductId, decimal Price) : ICommand<Result>;

public sealed class ChangeProductPriceCommandValidator : AbstractValidator<ChangeProductPriceCommand>
{
    public ChangeProductPriceCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Price).GreaterThan(0);
    }
}
