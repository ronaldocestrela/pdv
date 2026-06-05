using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Domain.Entities;

namespace Pdv.Infrastructure.Persistence.Configurations;

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Quantity).IsRequired();
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2).IsRequired();

        builder.HasOne(e => e.ProductVariation)
            .WithMany()
            .HasForeignKey(e => e.ProductVariationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.SaleId });
        builder.HasIndex(e => new { e.TenantId, e.ProductVariationId });
    }
}
