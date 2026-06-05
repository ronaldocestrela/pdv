using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pdv.Domain.Entities;

namespace Pdv.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configures database schema rules, keys, and indexes for EF Core.
    /// </summary>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();

        builder.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();

        builder.Property(e => e.RefreshToken).HasMaxLength(512);
        builder.Property(e => e.RefreshTokenExpiresAtUtc);

        builder.Property(e => e.IsActive).IsRequired();

        builder.HasIndex(e => e.TenantId);

        builder.HasMany(e => e.UserRoles)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
