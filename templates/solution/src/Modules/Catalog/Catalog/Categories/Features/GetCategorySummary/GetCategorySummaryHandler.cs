using Catalog.Categories.Errors;
using Catalog.Contracts.Categories;
using Wolverine.Attributes;

namespace Catalog.Categories.Features.GetCategorySummary;

/// <summary>Answers <see cref="GetCategorySummaryQuery"/>, part of the Catalog module's public API.</summary>
public static class GetCategorySummaryHandler
{
    [NonTransactional]
    public static async Task<Result<CategorySummaryDto>> Handle(
        GetCategorySummaryQuery query,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var summary = await dbContext.Categories
            .AsNoTracking()
            .Where(category => category.Id == query.CategoryId)
            .Select(GetCategorySummaryMapper.Projection)
            .FirstOrDefaultAsync(cancellationToken);

        return summary is null ? CategoryErrors.NotFound(query.CategoryId) : summary;
    }
}
