using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pdv.Shared.Kernel.Abstractions;
using Pdv.Modules.Stock.Domain.Entities;
using System.Linq.Expressions;

namespace Pdv.Modules.Stock.Infrastructure.Persistence;

public sealed class StockDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public StockDbContext(DbContextOptions<StockDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<ProductVariation> ProductVariations => Set<ProductVariation>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(BuildTenantFilterExpression(entityType));
        }
    }

    public Guid? CurrentTenantId => _tenantContext.TenantId;
    public bool CurrentIsSuperAdmin => _tenantContext.IsSuperAdmin;

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
            [typeof(Guid)],
            parameter,
            Expression.Constant(nameof(ITenantScoped.TenantId)));

        var dbContextExpr = Expression.Constant(this);
        var currentTenantExpr = Expression.Property(dbContextExpr, nameof(CurrentTenantId));
        var isSuperAdminExpr = Expression.Property(dbContextExpr, nameof(CurrentIsSuperAdmin));

        var hasTenantExpr = Expression.Property(currentTenantExpr, nameof(Nullable<Guid>.HasValue));
        var tenantValueExpr = Expression.Call(currentTenantExpr, nameof(Nullable<Guid>.GetValueOrDefault), Type.EmptyTypes);

        var tenantEqualsExpr = Expression.Equal(tenantIdProperty, tenantValueExpr);
        var scopedFilterExpr = Expression.AndAlso(hasTenantExpr, tenantEqualsExpr);
        var bypassExpr = Expression.OrElse(isSuperAdminExpr, Expression.Not(hasTenantExpr));
        var bodyExpr = Expression.OrElse(bypassExpr, scopedFilterExpr);

        return Expression.Lambda(bodyExpr, parameter);
    }
}
