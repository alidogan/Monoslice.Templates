using Catalog.Products.Errors;

namespace Catalog.Products.Features.CreateProduct;

public static class CreateProductHandler
{
    public static async Task<Result<CreateProductResult>> Handle(
        CreateProductCommand command,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Categories.AnyAsync(category => category.Id == command.CategoryId, cancellationToken))
        {
            return ProductErrors.CategoryNotFound(command.CategoryId);
        }

        var product = CreateProductMapper.ToEntity(command);
        dbContext.Products.Add(product);

        return CreateProductMapper.ToResult(product);
    }
}
