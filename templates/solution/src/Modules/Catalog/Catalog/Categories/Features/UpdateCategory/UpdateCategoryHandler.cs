using Catalog.Categories.Errors;
using Microsoft.Extensions.Caching.Hybrid;

namespace Catalog.Categories.Features.UpdateCategory;

public static class UpdateCategoryHandler
{
    public static async Task<Result> Handle(
        UpdateCategoryCommand command,
        CatalogDbContext dbContext,
        HybridCache cache,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(category => category.Id == command.Id, cancellationToken);

        if (category is null)
        {
            return CategoryErrors.NotFound(command.Id);
        }

        if (!dbContext.TryApplyExpectedVersion(category, command.ExpectedVersion))
        {
            return CategoryErrors.VersionMismatch(command.Id);
        }

        var name = command.Name.Trim();
        var nameTaken = await dbContext.Categories
            .AnyAsync(other => other.Id != command.Id && other.Name == name, cancellationToken);

        if (nameTaken)
        {
            return CategoryErrors.NameAlreadyExists(name);
        }

        UpdateCategoryMapper.Apply(command, category);
        await cache.RemoveAsync(CategoryCacheKeys.ById(command.Id), cancellationToken);

        return Result.Success();
    }
}
