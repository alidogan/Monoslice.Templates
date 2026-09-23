namespace Catalog.Categories.Features.DeleteCategory;

public static class DeleteCategoryMapper
{
    public static DeleteCategoryCommand ToCommand(Guid id, string expectedVersion) => new(id, expectedVersion);
}
