using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Domain.Entities;

namespace Pdv.Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.PaymentMethod).HasConversion<int>().IsRequired();

        builder.HasMany(e => e.Items)
            .WithOne(i => i.Sale)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.CashFlows)
            .WithOne(c => c.Sale)
            .HasForeignKey(c => c.SaleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
