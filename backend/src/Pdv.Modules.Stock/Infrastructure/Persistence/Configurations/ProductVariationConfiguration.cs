using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Modules.Stock.Domain.Entities;

namespace Pdv.Modules.Stock.Infrastructure.Persistence.Configurations;

public sealed class ProductVariationConfiguration : IEntityTypeConfiguration<ProductVariation>
{
    public void Configure(EntityTypeBuilder<ProductVariation> builder)
    {
        builder.ToTable("ProductVariations", t => t.ExcludeFromMigrations());
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId);
    }
}
