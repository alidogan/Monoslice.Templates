namespace Catalog.Categories.Errors;

public static class CategoryErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Catalog.Category.NotFound", $"Category '{id}' was not found.");

    public static Error NameAlreadyExists(string name) =>
        Error.Conflict("Catalog.Category.NameAlreadyExists", $"A category named '{name}' already exists.");

    public static Error VersionMismatch(Guid id) =>
        Error.PreconditionFailed(
            "Catalog.Category.VersionMismatch",
            $"Category '{id}' was changed since it was read. Reload it and try again.");
}
