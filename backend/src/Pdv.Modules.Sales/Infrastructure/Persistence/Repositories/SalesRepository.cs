using Microsoft.EntityFrameworkCore;
using Pdv.Modules.Sales.Application.Abstractions;
using Pdv.Modules.Sales.Domain.Entities;
using Pdv.Modules.Sales.Infrastructure.Persistence;

namespace Pdv.Modules.Sales.Infrastructure.Persistence.Repositories;

/// <summary>
/// Initializes a new instance of the <see cref="SalesRepository"/> class.
/// </summary>
public sealed class SalesRepository(SalesDbContext db) : ISaleRepository
{
    private readonly SalesDbContext _db = db;

    /// <summary>
    /// Adds a new entity to the database context.
    /// </summary>
    public void Add(Sale sale) =>
        _db.Sales.Add(sale);

    /// <summary>
    /// Adds a new entity to the database context.
    /// </summary>
    public void AddItem(SaleItem item) =>
        _db.SaleItems.Add(item);

    /// <summary>
    /// Adds a new entity to the database context.
    /// </summary>
    public void AddCashFlow(CashFlow cashFlow) =>
        _db.CashFlows.Add(cashFlow);

    /// <summary>
    /// Retrieves a list of DTO summaries.
    /// </summary>
    public async Task<IReadOnlyList<SaleListItemDto>> ListRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        var capped = Math.Clamp(take, 1, 500);
        return await _db.Sales.AsNoTracking()
            .OrderByDescending(s => s.CreatedAtUtc)
            .Take(capped)
            .Select(s => new SaleListItemDto(
                s.Id,
                s.CreatedAtUtc,
                s.TotalAmount,
                s.PaymentMethod,
                s.Items.Count))
            .ToListAsync(cancellationToken);
    }

    public Task<ProductVariation?> GetVariationByIdAsync(Guid variationId, CancellationToken cancellationToken = default) =>
        _db.ProductVariations.FirstOrDefaultAsync(v => v.Id == variationId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
