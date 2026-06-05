using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Domain.Entities;

namespace Pdv.Infrastructure.Persistence.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    /// <summary>
    /// Configures database schema rules, keys, and indexes for EF Core.
    /// </summary>
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Quantity).IsRequired();
        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.Reason).HasMaxLength(512);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.CreatedAtUtc);
        builder.HasIndex(e => new { e.TenantId, e.ProductVariationId });
        builder.HasIndex(e => new { e.TenantId, e.ProductVariationId, e.CreatedAtUtc });

        builder.HasOne(e => e.ProductVariation)
            .WithMany()
            .HasForeignKey(e => e.ProductVariationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
