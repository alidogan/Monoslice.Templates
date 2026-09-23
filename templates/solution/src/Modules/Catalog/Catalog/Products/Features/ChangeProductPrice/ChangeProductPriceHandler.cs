using Catalog.Products.Errors;

namespace Catalog.Products.Features.ChangeProductPrice;

public static class ChangeProductPriceHandler
{
    public static async Task<Result> Handle(
        ChangeProductPriceCommand command,
        CatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(product => product.Id == command.ProductId, cancellationToken);

        if (product is null)
        {
            return ProductErrors.NotFound(command.ProductId);
        }

        // Raises ProductPriceChangedDomainEvent; Wolverine publishes it after the transaction commits.
        return product.ChangePrice(command.Price);
    }
}
