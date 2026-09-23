using Shared.Contracts.Messaging;
using Shared.Contracts.Results;

namespace Catalog.Contracts.Categories;

/// <summary>
/// Lets other modules look up a category synchronously:
/// <c>await bus.InvokeAsync&lt;Result&lt;CategorySummaryDto&gt;&gt;(new GetCategorySummaryQuery(id), ct)</c>.
/// </summary>
public sealed record GetCategorySummaryQuery(Guid CategoryId) : IQuery<Result<CategorySummaryDto>>;

public sealed record CategorySummaryDto(Guid Id, string Name, bool IsActive);
