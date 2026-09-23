using Catalog.Categories.Models;

namespace Catalog.Categories.Features.UpdateCategory;

public static class UpdateCategoryMapper
{
    public static UpdateCategoryCommand ToCommand(Guid id, UpdateCategoryRequest request, string expectedVersion) =>
        new(id, request.Name, request.Description, request.DisplayOrder, request.IsActive, expectedVersion);

    public static void Apply(UpdateCategoryCommand command, CatalogCategory category) =>
        category.Update(command.Name, command.Description, command.DisplayOrder, command.IsActive);
}
