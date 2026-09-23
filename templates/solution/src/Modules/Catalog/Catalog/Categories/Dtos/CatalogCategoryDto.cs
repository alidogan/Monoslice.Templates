namespace Catalog.Categories.Dtos;

public sealed record CatalogCategoryDto(Guid Id, string Name, string? Description, int DisplayOrder, bool IsActive);
