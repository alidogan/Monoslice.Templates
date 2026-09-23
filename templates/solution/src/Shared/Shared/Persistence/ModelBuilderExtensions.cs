using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Shared.Persistence;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies the conventions every module shares: a soft delete query filter for <see cref="ISoftDeletable"/>
    /// and a concurrency token for <see cref="IHasConcurrencyToken"/>.
    /// </summary>
    public static ModelBuilder ApplySharedConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            if (entityType.IsOwned() || entityType.BaseType is not null)
            {
                continue;
            }

            var clrType = entityType.ClrType;

            if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
            {
                var parameter = Expression.Parameter(clrType, "entity");
                var notDeleted = Expression.Not(Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted)));
                modelBuilder.Entity(clrType)
                    .HasQueryFilter(DatabaseDefaults.SoftDeleteFilter, Expression.Lambda(notDeleted, parameter));
            }

            if (typeof(IHasConcurrencyToken).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType)
                    .Property(nameof(IHasConcurrencyToken.Version))
                    .IsRowVersion();
            }
        }

        return modelBuilder;
    }

    public static ModelBuilder ExcludeSchemaFromMigrations(this ModelBuilder modelBuilder, string schema)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (string.Equals(entityType.GetSchema(), schema, StringComparison.Ordinal))
            {
                entityType.SetIsTableExcludedFromMigrations(true);
            }
        }

        return modelBuilder;
    }
}
