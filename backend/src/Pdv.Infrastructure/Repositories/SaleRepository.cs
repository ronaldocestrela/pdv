using Microsoft.EntityFrameworkCore;
using Pdv.Application.Abstractions;
using Pdv.Domain.Entities;
using Pdv.Infrastructure.Persistence;

namespace Pdv.Infrastructure.Repositories;

/// <summary>
/// Initializes a new instance of the <see cref="SaleRepository"/> class.
/// </summary>
public sealed class SaleRepository(AppDbContext db) : ISaleRepository
{
    private readonly AppDbContext _db = db;

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
}
