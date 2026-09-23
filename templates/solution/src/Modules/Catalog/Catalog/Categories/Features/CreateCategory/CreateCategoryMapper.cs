using Catalog.Categories.Models;

namespace Catalog.Categories.Features.CreateCategory;

public static class CreateCategoryMapper
{
    public static CreateCategoryCommand ToCommand(CreateCategoryRequest request) =>
        new(request.Name, request.Description, request.DisplayOrder, request.IsActive);

    public static CatalogCategory ToEntity(CreateCategoryCommand command) =>
        CatalogCategory.Create(
            Guid.CreateVersion7(),
            command.Name,
            command.Description,
            command.DisplayOrder,
            command.IsActive);

    public static CreateCategoryResult ToResult(CatalogCategory category) => new(category.Id);

    public static CreateCategoryResponse ToResponse(CreateCategoryResult result) => new(result.Id);
}
