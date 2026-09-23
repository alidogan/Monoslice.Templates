using Catalog.Categories.Dtos;

namespace Catalog.Categories.Features.GetCategories;

public sealed record GetCategoriesQuery(PagedRequest Paging, string? Search)
    : IQuery<Result<PagedResult<CatalogCategoryDto>>>;
