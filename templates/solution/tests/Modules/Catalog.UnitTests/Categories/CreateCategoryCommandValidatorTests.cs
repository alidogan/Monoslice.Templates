using Catalog.Categories.Features.CreateCategory;
using Catalog.Categories.Models;

namespace Catalog.UnitTests.Categories;

public sealed class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Accepts_a_valid_command()
    {
        var result = _validator.Validate(new CreateCategoryCommand("Books", null, 0, IsActive: true));

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_an_empty_name(string name)
    {
        var result = _validator.Validate(new CreateCategoryCommand(name, null, 0, IsActive: true));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Fact]
    public void Rejects_a_name_that_is_too_long()
    {
        var name = new string('x', CatalogCategory.NameMaxLength + 1);

        var result = _validator.Validate(new CreateCategoryCommand(name, null, 0, IsActive: true));

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Rejects_a_negative_display_order()
    {
        var result = _validator.Validate(new CreateCategoryCommand("Books", null, -1, IsActive: true));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateCategoryCommand.DisplayOrder));
    }
}
