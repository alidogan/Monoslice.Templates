using System.Linq.Expressions;
using Catalog.Categories.Dtos;
using Catalog.Categories.Models;

namespace Catalog.Categories.Features.GetCategories;

public static class GetCategoriesMapper
{
    /// <summary>Translated to SQL, so only the columns of the DTO are selected.</summary>
    public static readonly Expression<Func<CatalogCategory, CatalogCategoryDto>> Projection = category =>
        new CatalogCategoryDto(category.Id, category.Name, category.Description, category.DisplayOrder, category.IsActive);

    public static GetCategoriesQuery ToQuery(PagedRequest paging, string? search) => new(paging, search);
}
