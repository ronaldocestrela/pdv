using Microsoft.EntityFrameworkCore;
using Pdv.Application.Abstractions;
using Pdv.Domain.Entities;
using Pdv.Domain.Enums;
using Pdv.Infrastructure.Persistence;

namespace Pdv.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProductSummaryDto>> ListSummariesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Products.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProductSummaryDto(p.Id, p.Name, p.IsActive, p.Variations.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDetailDto?> GetDetailByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await _db.Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.IsActive,
                Variations = p.Variations
                    .OrderBy(v => v.Name)
                    .Select(v => new ProductVariationDto(v.Id, v.ProductId, v.Name, v.Barcode, v.StockQuantity, v.UnitPrice))
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return null;

        return new ProductDetailDto(row.Id, row.Name, row.IsActive, row.Variations);
    }

    public Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default) =>
        _db.Products.AsNoTracking().AnyAsync(p => p.Id == productId, cancellationToken);

    public Task<Product?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public void Add(Product product) =>
        _db.Products.Add(product);

    public void Remove(Product product) =>
        _db.Products.Remove(product);

    public Task<ProductVariation?> GetTrackedVariationByIdAsync(int variationId, CancellationToken cancellationToken = default)
    {
        return _db.ProductVariations.FirstOrDefaultAsync(v => v.Id == variationId, cancellationToken);
    }

    public void AddVariation(ProductVariation variation) =>
        _db.ProductVariations.Add(variation);

    public void RemoveVariation(ProductVariation variation) =>
        _db.ProductVariations.Remove(variation);

    public async Task<bool> IsBarcodeTakenAsync(string barcode, int? excludeVariationId, CancellationToken cancellationToken = default)
    {
        var trimmed = barcode.Trim();
        var q = _db.ProductVariations.Where(v => v.Barcode != null && v.Barcode == trimmed);
        if (excludeVariationId.HasValue)
            q = q.Where(v => v.Id != excludeVariationId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockMovementListItemDto>> ListStockMovementsAsync(
        int? productVariationId,
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

    public void AddStockMovement(StockMovement movement) =>
        _db.StockMovements.Add(movement);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
