using Catalog.Categories.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Data.Configurations;

internal sealed class CatalogCategoryConfiguration : IEntityTypeConfiguration<CatalogCategory>
{
    public void Configure(EntityTypeBuilder<CatalogCategory> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedNever();
        builder.Property(category => category.Name).HasMaxLength(CatalogCategory.NameMaxLength).IsRequired();
        builder.Property(category => category.Description).HasMaxLength(CatalogCategory.DescriptionMaxLength);
        builder.Property(category => category.CreatedBy).HasMaxLength(200);
        builder.Property(category => category.LastModifiedBy).HasMaxLength(200);
        builder.Property(category => category.DeletedBy).HasMaxLength(200);

        // Names are unique among categories that are not (soft) deleted.
        builder.HasIndex(category => category.Name)
            .IsUnique()
#if (UsePostgreSQL)
            .HasFilter("\"IsDeleted\" = false");
#else
            .HasFilter("[IsDeleted] = 0");
#endif
    }
}
