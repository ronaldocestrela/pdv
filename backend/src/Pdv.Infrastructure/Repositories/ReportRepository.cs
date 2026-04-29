using Microsoft.EntityFrameworkCore;
using Pdv.Application.Abstractions;
using Pdv.Infrastructure.Persistence;

namespace Pdv.Infrastructure.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly AppDbContext _db;

    public ReportRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SalesReportDto> GetSalesSummaryAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var q = _db.Sales.AsNoTracking().Where(s => s.CreatedAtUtc >= fromUtc && s.CreatedAtUtc <= toUtc);
        var saleCount = await q.CountAsync(cancellationToken);
        var total = saleCount == 0 ? 0m : await q.SumAsync(s => s.TotalAmount, cancellationToken);
        return new SalesReportDto(saleCount, total);
    }

    public async Task<IReadOnlyList<TopProductReportDto>> GetTopProductsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int take,
        CancellationToken cancellationToken = default)
    {
        var capped = Math.Clamp(take, 1, 100);

        // EF Core cannot translate every GroupBy/join shape across providers; project rows then aggregate in-memory (OK for MVP volumes).
        var lines = await _db.SaleItems.AsNoTracking()
            .Where(si => si.Sale.CreatedAtUtc >= fromUtc && si.Sale.CreatedAtUtc <= toUtc)
            .Select(si => new
            {
                si.ProductVariationId,
                ProductName = si.ProductVariation.Product.Name,
                VariationName = si.ProductVariation.Name,
                si.Quantity,
                LineTotal = si.Quantity * si.UnitPrice,
            })
            .ToListAsync(cancellationToken);

        return lines
            .GroupBy(x => new { x.ProductVariationId, x.ProductName, x.VariationName })
            .Select(g => new TopProductReportDto(
                g.Key.ProductVariationId,
                g.Key.ProductName,
                g.Key.VariationName,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.LineTotal)))
            .OrderByDescending(x => x.QuantitySold)
            .Take(capped)
            .ToList();
    }

    public async Task<IReadOnlyList<CashFlowReportRowDto>> ListCashFlowsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int take,
        CancellationToken cancellationToken = default)
    {
        var capped = Math.Clamp(take, 1, 500);

        return await _db.CashFlows.AsNoTracking()
            .Where(c => c.CreatedAtUtc >= fromUtc && c.CreatedAtUtc <= toUtc)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(capped)
            .Select(c => new CashFlowReportRowDto(c.Id, c.Type, c.Amount, c.Description, c.CreatedAtUtc, c.SaleId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockReportRowDto>> ListStockLevelsAsync(int take, CancellationToken cancellationToken = default)
    {
        var capped = Math.Clamp(take, 1, 500);

        return await _db.ProductVariations.AsNoTracking()
            .OrderBy(v => v.Product.Name).ThenBy(v => v.Name)
            .Take(capped)
            .Select(v => new StockReportRowDto(v.Id, v.Product.Name, v.Name, v.StockQuantity))
            .ToListAsync(cancellationToken);
    }
}
