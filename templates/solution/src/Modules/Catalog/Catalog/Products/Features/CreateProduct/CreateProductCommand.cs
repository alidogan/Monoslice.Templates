using Catalog.Products.Models;

namespace Catalog.Products.Features.CreateProduct;

public sealed record CreateProductCommand(Guid CategoryId, string Name, string? Description, decimal Price)
    : ICommand<Result<CreateProductResult>>;

public sealed record CreateProductResult(Guid Id);

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.CategoryId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(command => command.Description).MaximumLength(Product.DescriptionMaxLength);
        RuleFor(command => command.Price).GreaterThan(0);
    }
}
