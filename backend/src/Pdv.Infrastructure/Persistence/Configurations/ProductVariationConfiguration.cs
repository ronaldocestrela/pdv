using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Domain.Entities;

namespace Pdv.Infrastructure.Persistence.Configurations;

public sealed class ProductVariationConfiguration : IEntityTypeConfiguration<ProductVariation>
{
    /// <summary>
    /// Configures database schema rules, keys, and indexes for EF Core.
    /// </summary>
    public void Configure(EntityTypeBuilder<ProductVariation> builder)
    {
        builder.ToTable("ProductVariations");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Barcode).HasMaxLength(64);
        builder.Property(e => e.StockQuantity).IsRequired();
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2).IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.ProductId });
        builder.HasIndex(e => e.TenantId);

        builder.HasIndex(e => new { e.TenantId, e.Barcode })
            .IsUnique()
            .HasFilter("[Barcode] IS NOT NULL");
    }
}
