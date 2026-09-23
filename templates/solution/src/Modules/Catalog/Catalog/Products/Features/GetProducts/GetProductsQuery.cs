using Catalog.Products.Dtos;

namespace Catalog.Products.Features.GetProducts;

public sealed record GetProductsQuery(PagedRequest Paging, Guid? CategoryId) : IQuery<Result<PagedResult<ProductDto>>>;
