using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain;
using Shared.Security;

namespace Shared.Persistence;

/// <summary>Fills the <see cref="IAuditable"/> columns with the current user and time.</summary>
public sealed class AuditableEntityInterceptor(ICurrentUser currentUser, TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        var user = currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue = user;
            }
            else if (entry.State == EntityState.Modified || HasChangedOwnedEntities(entry))
            {
                entry.Property(nameof(IAuditable.LastModifiedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditable.LastModifiedBy)).CurrentValue = user;
            }
        }
    }

    private static bool HasChangedOwnedEntities(EntityEntry entry) =>
        entry.References.Any(reference =>
            reference.TargetEntry is { } target
            && target.Metadata.IsOwned()
            && target.State is EntityState.Added or EntityState.Modified);
}
