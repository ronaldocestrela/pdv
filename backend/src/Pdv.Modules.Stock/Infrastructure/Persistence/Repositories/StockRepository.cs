using Microsoft.EntityFrameworkCore;
using Pdv.Modules.Stock.Application.Abstractions;
using Pdv.Modules.Stock.Domain.Entities;
using Pdv.Modules.Stock.Infrastructure.Persistence;
using Pdv.Shared.Kernel.Enums;

namespace Pdv.Modules.Stock.Infrastructure.Persistence.Repositories;

/// <summary>
/// Initializes a new instance of the <see cref="StockRepository"/> class.
/// </summary>
public sealed class StockRepository(StockDbContext db) : IStockRepository
{
    private readonly StockDbContext _db = db;

    public async Task<IReadOnlyList<StockMovementListItemDto>> ListStockMovementsAsync(
        Guid? productVariationId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var q = _db.StockMovements.AsNoTracking().AsQueryable();
        if (productVariationId.HasValue)
            q = q.Where(m => m.ProductVariationId == productVariationId.Value);

        var cappedTake = Math.Clamp(take, 1, 500);

        return await q
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(cappedTake)
            .Select(m => new StockMovementListItemDto(
                m.Id,
                m.ProductVariationId,
                m.ProductVariation.Product.Name,
                m.ProductVariation.Name,
                m.Type == StockMovementType.In ? "IN" : "OUT",
                m.Quantity,
                m.CreatedAtUtc,
                m.Reason))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new entity to the database context.
    /// </summary>
    public void AddStockMovement(StockMovement movement) =>
        _db.StockMovements.Add(movement);

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public Task<ProductVariation?> GetTrackedVariationByIdAsync(Guid variationId, CancellationToken cancellationToken = default)
    {
        return _db.ProductVariations.FirstOrDefaultAsync(v => v.Id == variationId, cancellationToken);
    }

    /// <summary>
    /// Persists all tracked changes in this database context.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
