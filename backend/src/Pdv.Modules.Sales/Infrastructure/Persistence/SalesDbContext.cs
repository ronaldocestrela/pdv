using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pdv.Shared.Kernel.Abstractions;
using Pdv.Modules.Sales.Domain.Entities;
using System.Linq.Expressions;

namespace Pdv.Modules.Sales.Infrastructure.Persistence;

public sealed class SalesDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public SalesDbContext(DbContextOptions<SalesDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<CashFlow> CashFlows => Set<CashFlow>();
    public DbSet<ProductVariation> ProductVariations => Set<ProductVariation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(BuildTenantFilterExpression(entityType));
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantScope();
        return base.SaveChangesAsync(cancellationToken);
    }

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
