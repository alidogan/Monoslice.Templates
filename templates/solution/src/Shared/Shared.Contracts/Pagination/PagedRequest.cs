namespace Shared.Contracts.Pagination;

/// <summary>1-based paging parameters, bound from the query string with <c>[AsParameters]</c>.</summary>
public sealed record PagedRequest(int Page = 1, int PageSize = PagedRequest.DefaultPageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int NormalizedPage => Math.Max(1, Page);

    public int NormalizedPageSize => Math.Clamp(PageSize, 1, MaxPageSize);
}
