using Catalog.Categories.Features.CreateCategory;

namespace Catalog.UnitTests.Categories;

public sealed class CreateCategoryMapperTests
{
    [Fact]
    public void Maps_request_to_command_to_entity_to_result()
    {
        var request = new CreateCategoryRequest("Books", "All books", 1, IsActive: true);

        var command = CreateCategoryMapper.ToCommand(request);
        var entity = CreateCategoryMapper.ToEntity(command);
        var result = CreateCategoryMapper.ToResult(entity);
        var response = CreateCategoryMapper.ToResponse(result);

        command.ShouldBe(new CreateCategoryCommand("Books", "All books", 1, IsActive: true));
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.Name.ShouldBe("Books");
        entity.Description.ShouldBe("All books");
        entity.DisplayOrder.ShouldBe(1);
        entity.IsActive.ShouldBeTrue();
        result.Id.ShouldBe(entity.Id);
        response.Id.ShouldBe(entity.Id);
    }

    [Fact]
    public void Generates_time_ordered_ids()
    {
        var command = new CreateCategoryCommand("Books", null, 0, IsActive: true);

        var first = CreateCategoryMapper.ToEntity(command);
        var second = CreateCategoryMapper.ToEntity(command);

        first.Id.Version.ShouldBe(7);
        second.Id.ShouldNotBe(first.Id);
    }
}
