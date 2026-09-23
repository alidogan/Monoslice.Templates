using Catalog.Categories.Models;

namespace Catalog.UnitTests.Categories;

public sealed class CatalogCategoryTests
{
    [Fact]
    public void Create_sets_all_fields_and_trims_text()
    {
        var id = Guid.CreateVersion7();

        var category = CatalogCategory.Create(id, "  Books ", " All books ", 3, isActive: true);

        category.Id.ShouldBe(id);
        category.Name.ShouldBe("Books");
        category.Description.ShouldBe("All books");
        category.DisplayOrder.ShouldBe(3);
        category.IsActive.ShouldBeTrue();
        category.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Update_replaces_the_editable_fields()
    {
        var category = CatalogCategory.Create(Guid.CreateVersion7(), "Books", null, 1, isActive: true);

        category.Update("Novels", "Fiction", 2, isActive: false);

        category.Name.ShouldBe("Novels");
        category.Description.ShouldBe("Fiction");
        category.DisplayOrder.ShouldBe(2);
        category.IsActive.ShouldBeFalse();
    }
}
