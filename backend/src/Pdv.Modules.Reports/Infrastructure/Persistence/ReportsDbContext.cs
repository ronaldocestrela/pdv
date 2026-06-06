using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pdv.Shared.Kernel.Abstractions;
using Pdv.Modules.Reports.Domain.Entities;
using System.Linq.Expressions;

namespace Pdv.Modules.Reports.Infrastructure.Persistence;

public sealed class ReportsDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ReportsDbContext(DbContextOptions<ReportsDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariation> ProductVariations => Set<ProductVariation>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<CashFlow> CashFlows => Set<CashFlow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportsDbContext).Assembly);

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
