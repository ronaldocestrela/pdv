using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pdv.Application.Abstractions;
using Pdv.Domain.Entities;
using System.Linq.Expressions;

namespace Pdv.Infrastructure.Persistence;

/// <summary>
/// Initializes a new instance of the <see cref="AppDbContext"/> class.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext) : DbContext(options)
{
    private readonly ITenantContext _tenantContext = tenantContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : this(options, new Services.SystemTenantContext())
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariation> ProductVariations => Set<ProductVariation>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<CashFlow> CashFlows => Set<CashFlow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(BuildTenantFilterExpression(entityType));
        }
    }

    /// <summary>
    /// Executes the SaveChangesAsync operation.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantScope();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Executes the SaveChanges operation.
    /// </summary>
    public override int SaveChanges()
    {
        ApplyTenantScope();
        return base.SaveChanges();
    }

    private void ApplyTenantScope()
    {
        if (_tenantContext.IsSuperAdmin)
            return;

        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue)
            return;

        foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = tenantId.Value;
            }
            else if (entry.State == EntityState.Modified)
            {
                var originalTenantId = entry.Property(e => e.TenantId).OriginalValue;
                if (originalTenantId != tenantId.Value)
                    throw new InvalidOperationException("Cross-tenant update is not allowed.");

                entry.Entity.TenantId = tenantId.Value;
            }
        }
    }

    private LambdaExpression BuildTenantFilterExpression(IMutableEntityType entityType)
    {
        var parameter = Expression.Parameter(entityType.ClrType, "entity");

        var tenantIdProperty = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            [typeof(int)],
            parameter,
            Expression.Constant(nameof(ITenantScoped.TenantId)));

        var tenantContextExpr = Expression.Constant(this);
        var currentTenantExpr = Expression.Property(
            Expression.Field(tenantContextExpr, "_tenantContext"),
            nameof(ITenantContext.TenantId));
        var isSuperAdminExpr = Expression.Property(
            Expression.Field(tenantContextExpr, "_tenantContext"),
            nameof(ITenantContext.IsSuperAdmin));

        var hasTenantExpr = Expression.Property(currentTenantExpr, nameof(Nullable<int>.HasValue));
        var tenantValueExpr = Expression.Call(currentTenantExpr, nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes);

        var tenantEqualsExpr = Expression.Equal(tenantIdProperty, tenantValueExpr);
        var scopedFilterExpr = Expression.AndAlso(hasTenantExpr, tenantEqualsExpr);
        var bypassExpr = Expression.OrElse(isSuperAdminExpr, Expression.Not(hasTenantExpr));
        var bodyExpr = Expression.OrElse(bypassExpr, scopedFilterExpr);

        return Expression.Lambda(bodyExpr, parameter);
    }
}
