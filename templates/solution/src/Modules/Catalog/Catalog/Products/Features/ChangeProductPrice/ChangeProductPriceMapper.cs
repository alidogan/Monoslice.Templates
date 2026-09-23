namespace Catalog.Products.Features.ChangeProductPrice;

public static class ChangeProductPriceMapper
{
    public static ChangeProductPriceCommand ToCommand(Guid productId, ChangeProductPriceRequest request) =>
        new(productId, request.Price);
}
