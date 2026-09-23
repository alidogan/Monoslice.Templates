using Catalog.Products.Models;

namespace Catalog.Products.Features.CreateProduct;

public static class CreateProductMapper
{
    public static CreateProductCommand ToCommand(CreateProductRequest request) =>
        new(request.CategoryId, request.Name, request.Description, request.Price);

    public static Product ToEntity(CreateProductCommand command) =>
        Product.Create(
            Guid.CreateVersion7(),
            command.CategoryId,
            command.Name,
            command.Description,
            command.Price);

    public static CreateProductResult ToResult(Product product) => new(product.Id);

    public static CreateProductResponse ToResponse(CreateProductResult result) => new(result.Id);
}
