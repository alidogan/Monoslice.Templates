using Catalog.Categories.Dtos;

namespace Catalog.Categories.Features.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<Result<GetCategoryByIdResult>>;

public sealed record GetCategoryByIdResult(CatalogCategoryDto Category, string ETag);
