using Catalog.Categories.Dtos;
using Wolverine.Attributes;

namespace Catalog.Categories.Features.GetCategories;

public static class GetCategoriesHandler
{
    [NonTransactional]
    public static async Task<Result<PagedResult<CatalogCategoryDto>>> Handle(
        GetCategoriesQuery query,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var categories = dbContext.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            categories = categories.Where(category => category.Name.Contains(search));
        }

        return await categories
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select(GetCategoriesMapper.Projection)
            .ToPagedResultAsync(query.Paging, cancellationToken);
    }
}
