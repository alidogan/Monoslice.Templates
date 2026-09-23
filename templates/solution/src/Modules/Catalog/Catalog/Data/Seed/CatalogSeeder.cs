using Catalog.Categories.Models;
using Catalog.Products.Models;

namespace Catalog.Data.Seed;

internal sealed class CatalogSeeder(CatalogDbContext dbContext) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Categories.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        var books = CatalogCategory.Create(Guid.CreateVersion7(), "Books", "Printed and digital books", 1, isActive: true);
        var games = CatalogCategory.Create(Guid.CreateVersion7(), "Games", "Board and video games", 2, isActive: true);

        dbContext.Categories.AddRange(books, games);
        dbContext.Products.AddRange(
            Product.Create(Guid.CreateVersion7(), books.Id, "Domain-Driven Design", "Eric Evans", 54.99m),
            Product.Create(Guid.CreateVersion7(), games.Id, "Chess", "Wooden chess set", 39.95m));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
