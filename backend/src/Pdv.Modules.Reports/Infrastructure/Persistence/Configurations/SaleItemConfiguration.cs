using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Modules.Reports.Domain.Entities;

namespace Pdv.Modules.Reports.Infrastructure.Persistence.Configurations;

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems", t => t.ExcludeFromMigrations());
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);

        builder.HasOne(e => e.Sale)
            .WithMany()
            .HasForeignKey(e => e.SaleId);

        builder.HasOne(e => e.ProductVariation)
            .WithMany()
            .HasForeignKey(e => e.ProductVariationId);
    }
}
