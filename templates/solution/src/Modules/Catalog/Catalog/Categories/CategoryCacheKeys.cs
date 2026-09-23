namespace Catalog.Categories;

internal static class CategoryCacheKeys
{
    public const string Tag = "catalog:categories";

    public static string ById(Guid id) => $"catalog:categories:{id}";
}
