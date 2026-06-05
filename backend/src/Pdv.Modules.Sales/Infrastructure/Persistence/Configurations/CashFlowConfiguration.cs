using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Modules.Sales.Domain.Entities;

namespace Pdv.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class CashFlowConfiguration : IEntityTypeConfiguration<CashFlow>
{
    /// <summary>
    /// Configures database schema rules, keys, and indexes for EF Core.
    /// </summary>
    public void Configure(EntityTypeBuilder<CashFlow> builder)
    {
        builder.ToTable("CashFlows");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Type).HasConversion<int>().IsRequired();
        builder.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(512).IsRequired();
        builder.Property(e => e.CreatedAtUtc).IsRequired();

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.CreatedAtUtc);
        builder.HasIndex(e => new { e.TenantId, e.SaleId });
    }
}
