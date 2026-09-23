using Catalog.Categories.Models;

namespace Catalog.Categories.Features.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string? Description, int DisplayOrder, bool IsActive)
    : ICommand<Result<CreateCategoryResult>>;

public sealed record CreateCategoryResult(Guid Id);

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(CatalogCategory.NameMaxLength);
        RuleFor(command => command.Description).MaximumLength(CatalogCategory.DescriptionMaxLength);
        RuleFor(command => command.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
