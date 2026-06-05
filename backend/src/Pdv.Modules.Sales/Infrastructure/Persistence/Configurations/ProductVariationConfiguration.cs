using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Modules.Sales.Domain.Entities;

namespace Pdv.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class ProductVariationConfiguration : IEntityTypeConfiguration<ProductVariation>
{
    public void Configure(EntityTypeBuilder<ProductVariation> builder)
    {
        builder.ToTable("ProductVariations", t => t.ExcludeFromMigrations());
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
    }
}
