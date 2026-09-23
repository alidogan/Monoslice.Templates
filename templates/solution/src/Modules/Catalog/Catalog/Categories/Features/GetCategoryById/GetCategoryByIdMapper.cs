using Catalog.Categories.Dtos;
using Catalog.Categories.Models;

namespace Catalog.Categories.Features.GetCategoryById;

public static class GetCategoryByIdMapper
{
    public static GetCategoryByIdQuery ToQuery(Guid id) => new(id);

    public static GetCategoryByIdResult ToResult(CatalogCategory category) =>
        new(ToDto(category), ConcurrencyToken.ToETag(category));

    public static CatalogCategoryDto ToDto(CatalogCategory category) =>
        new(category.Id, category.Name, category.Description, category.DisplayOrder, category.IsActive);
}
