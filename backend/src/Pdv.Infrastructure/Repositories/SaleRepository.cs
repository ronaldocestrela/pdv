using Microsoft.EntityFrameworkCore;
using Pdv.Application.Abstractions;
using Pdv.Domain.Entities;
using Pdv.Infrastructure.Persistence;

namespace Pdv.Infrastructure.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly AppDbContext _db;

    public SaleRepository(AppDbContext db)
    {
        _db = db;
    }

    public void Add(Sale sale) =>
        _db.Sales.Add(sale);

    public void AddItem(SaleItem item) =>
        _db.SaleItems.Add(item);

    public void AddCashFlow(CashFlow cashFlow) =>
        _db.CashFlows.Add(cashFlow);

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
