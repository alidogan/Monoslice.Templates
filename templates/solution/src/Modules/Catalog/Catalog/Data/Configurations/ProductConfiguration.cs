using Catalog.Products.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Data.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();
        builder.Property(product => product.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(Product.DescriptionMaxLength);
        builder.Property(product => product.Price).HasPrecision(18, 2);
        builder.Property(product => product.CreatedBy).HasMaxLength(200);
        builder.Property(product => product.LastModifiedBy).HasMaxLength(200);

        // No foreign key to Categories: categories are soft deleted, so the row always stays.
        builder.HasIndex(product => product.CategoryId);
    }
}
