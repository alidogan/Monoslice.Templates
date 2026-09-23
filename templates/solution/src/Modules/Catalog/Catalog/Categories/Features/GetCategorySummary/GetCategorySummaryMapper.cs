using System.Linq.Expressions;
using Catalog.Categories.Models;
using Catalog.Contracts.Categories;

namespace Catalog.Categories.Features.GetCategorySummary;

public static class GetCategorySummaryMapper
{
    public static readonly Expression<Func<CatalogCategory, CategorySummaryDto>> Projection = category =>
        new CategorySummaryDto(category.Id, category.Name, category.IsActive);
}
