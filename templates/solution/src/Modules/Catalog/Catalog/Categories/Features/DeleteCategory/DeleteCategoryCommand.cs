namespace Catalog.Categories.Features.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id, string ExpectedVersion) : ICommand<Result>;

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.ExpectedVersion).NotEmpty();
    }
}
