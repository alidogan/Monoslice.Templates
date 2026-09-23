using Catalog.Categories.Models;

namespace Catalog.Categories.Features.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    string ExpectedVersion) : ICommand<Result>;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(CatalogCategory.NameMaxLength);
        RuleFor(command => command.Description).MaximumLength(CatalogCategory.DescriptionMaxLength);
        RuleFor(command => command.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(command => command.ExpectedVersion).NotEmpty();
    }
}
