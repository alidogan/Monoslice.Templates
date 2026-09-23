using Catalog.Categories.Errors;
using Microsoft.Extensions.Caching.Hybrid;

namespace Catalog.Categories.Features.DeleteCategory;

public static class DeleteCategoryHandler
{
    public static async Task<Result> Handle(
        DeleteCategoryCommand command,
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

        // Soft delete: SoftDeleteInterceptor turns this into an update that sets IsDeleted.
        dbContext.Categories.Remove(category);
        await cache.RemoveAsync(CategoryCacheKeys.ById(command.Id), cancellationToken);

        return Result.Success();
    }
}
