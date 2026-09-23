using Catalog.Categories.Errors;
using Microsoft.Extensions.Caching.Hybrid;
using Wolverine.Attributes;

namespace Catalog.Categories.Features.GetCategoryById;

public static class GetCategoryByIdHandler
{
    [NonTransactional]
    public static async Task<Result<GetCategoryByIdResult>> Handle(
        GetCategoryByIdQuery query,
        CatalogDbContext dbContext,
        HybridCache cache,
        CancellationToken cancellationToken)
    {
        var result = await cache.GetOrCreateAsync(
            CategoryCacheKeys.ById(query.Id),
            async token =>
            {
                var category = await dbContext.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(category => category.Id == query.Id, token);

                return category is null ? null : GetCategoryByIdMapper.ToResult(category);
            },
            tags: [CategoryCacheKeys.Tag],
            cancellationToken: cancellationToken);

        return result is null ? CategoryErrors.NotFound(query.Id) : result;
    }
}
