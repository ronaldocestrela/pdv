using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Modules.Catalog.Domain.Entities;

namespace Pdv.Modules.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration rules for the <see cref="Supplier"/> entity.
/// </summary>
public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    /// <summary>
    /// Configures the database schema, primary key, properties, and indexes for the Supplier entity.
    /// </summary>
    /// <param name="builder">The builder to configure the entity.</param>
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Document).HasMaxLength(32);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.IsActive).IsRequired();

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.Name });
        
        builder.HasIndex(e => new { e.TenantId, e.Document })
            .IsUnique()
            .HasFilter("[Document] IS NOT NULL");
    }
}
