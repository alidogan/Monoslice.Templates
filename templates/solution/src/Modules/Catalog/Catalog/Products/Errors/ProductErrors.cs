namespace Catalog.Products.Errors;

public static class ProductErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Catalog.Product.NotFound", $"Product '{id}' was not found.");

    public static Error CategoryNotFound(Guid categoryId) =>
        Error.Validation("Catalog.Product.CategoryNotFound", $"Category '{categoryId}' does not exist.");

    public static Error InvalidPrice(decimal price) =>
        Error.Validation("Catalog.Product.InvalidPrice", $"Price must be greater than zero, but was {price}.");
}
