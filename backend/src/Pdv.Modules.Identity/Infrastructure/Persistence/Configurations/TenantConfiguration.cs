using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Modules.Identity.Infrastructure.Configurations;

/// <summary>
/// Configura o mapeamento da entidade <see cref="Tenant"/> no banco de dados.
/// </summary>
public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    /// <summary>
    /// Define tabela, chave primária, restrições e índices para a entidade Tenant.
    /// </summary>
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();
    }
}
