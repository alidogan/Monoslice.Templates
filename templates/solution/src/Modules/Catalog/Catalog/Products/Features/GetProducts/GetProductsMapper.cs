using System.Linq.Expressions;
using Catalog.Products.Dtos;
using Catalog.Products.Models;

namespace Catalog.Products.Features.GetProducts;

public static class GetProductsMapper
{
    public static readonly Expression<Func<Product, ProductDto>> Projection = product =>
        new ProductDto(product.Id, product.CategoryId, product.Name, product.Description, product.Price);

    public static GetProductsQuery ToQuery(PagedRequest paging, Guid? categoryId) => new(paging, categoryId);
}
