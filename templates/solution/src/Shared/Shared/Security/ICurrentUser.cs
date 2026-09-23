namespace Shared.Security;

public interface ICurrentUser
{
    /// <summary>The authenticated user's id, or <c>"system"</c> outside an authenticated request.</summary>
    string UserId { get; }

    bool IsAuthenticated { get; }
}
