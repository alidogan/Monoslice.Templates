using Catalog.Products.Dtos;
using Wolverine.Attributes;

namespace Catalog.Products.Features.GetProducts;

public static class GetProductsHandler
{
    [NonTransactional]
    public static async Task<Result<PagedResult<ProductDto>>> Handle(
        GetProductsQuery query,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var products = dbContext.Products.AsNoTracking();

        if (query.CategoryId is { } categoryId)
        {
            products = products.Where(product => product.CategoryId == categoryId);
        }

        return await products
            .OrderBy(product => product.Name)
            .Select(GetProductsMapper.Projection)
            .ToPagedResultAsync(query.Paging, cancellationToken);
    }
}
