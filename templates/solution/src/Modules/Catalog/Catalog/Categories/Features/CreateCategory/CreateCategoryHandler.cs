using Catalog.Categories.Errors;

namespace Catalog.Categories.Features.CreateCategory;

public static class CreateCategoryHandler
{
    public static async Task<Result<CreateCategoryResult>> Handle(
        CreateCategoryCommand command,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var name = command.Name.Trim();
        if (await dbContext.Categories.AnyAsync(category => category.Name == name, cancellationToken))
        {
            return CategoryErrors.NameAlreadyExists(name);
        }

        var category = CreateCategoryMapper.ToEntity(command);
        dbContext.Categories.Add(category);

        // No SaveChangesAsync: Wolverine's transactional middleware commits after the handler returns.
        return CreateCategoryMapper.ToResult(category);
    }
}
