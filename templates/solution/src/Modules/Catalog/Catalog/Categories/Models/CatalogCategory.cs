namespace Catalog.Categories.Models;

public sealed class CatalogCategory : AggregateRoot<Guid>, ISoftDeletable
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private CatalogCategory()
    {
    }

    private CatalogCategory(Guid id, string name, string? description, int displayOrder, bool isActive)
        : base(id)
    {
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public string? DeletedBy { get; private set; }

    public static CatalogCategory Create(Guid id, string name, string? description, int displayOrder, bool isActive) =>
        new(id, name.Trim(), description?.Trim(), displayOrder, isActive);

    public void Update(string name, string? description, int displayOrder, bool isActive)
    {
        Name = name.Trim();
        Description = description?.Trim();
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }
}
