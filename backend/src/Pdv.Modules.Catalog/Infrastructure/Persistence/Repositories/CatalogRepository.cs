using Microsoft.EntityFrameworkCore;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Domain.Entities;
using Pdv.Modules.Catalog.Infrastructure.Persistence;

namespace Pdv.Modules.Catalog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Initializes a new instance of the <see cref="CatalogRepository"/> class.
/// </summary>
public sealed class CatalogRepository(CatalogDbContext db) : ICatalogRepository
{
    private readonly CatalogDbContext _db = db;

    /// <summary>
    /// Retrieves a list of DTO summaries.
    /// </summary>
    public async Task<IReadOnlyList<ProductSummaryDto>> ListSummariesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Products.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProductSummaryDto(p.Id, p.Name, p.IsActive, p.Variations.Count))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
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

    /// <summary>
    /// Executes the ProductExistsAsync operation.
    /// </summary>
    public Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default) =>
        _db.Products.AsNoTracking().AnyAsync(p => p.Id == productId, cancellationToken);

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public Task<Product?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <summary>
    /// Adds a new entity to the database context.
    /// </summary>
    public void Add(Product product) =>
        _db.Products.Add(product);

    /// <summary>
    /// Removes the specified entity from the database context.
    /// </summary>
    public void Remove(Product product) =>
        _db.Products.Remove(product);

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public Task<ProductVariation?> GetTrackedVariationByIdAsync(int variationId, CancellationToken cancellationToken = default)
    {
        return _db.ProductVariations.FirstOrDefaultAsync(v => v.Id == variationId, cancellationToken);
    }

    /// <summary>
    /// Adds a new entity to the database context.
    /// </summary>
    public void AddVariation(ProductVariation variation) =>
        _db.ProductVariations.Add(variation);

    /// <summary>
    /// Removes the specified entity from the database context.
    /// </summary>
    public void RemoveVariation(ProductVariation variation) =>
        _db.ProductVariations.Remove(variation);

    /// <summary>
    /// Executes the IsBarcodeTakenAsync operation.
    /// </summary>
    public async Task<bool> IsBarcodeTakenAsync(string barcode, int? excludeVariationId, CancellationToken cancellationToken = default)
    {
        var trimmed = barcode.Trim();
        var q = _db.ProductVariations.Where(v => v.Barcode != null && v.Barcode == trimmed);
        if (excludeVariationId.HasValue)
            q = q.Where(v => v.Id != excludeVariationId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Persists all tracked changes in this database context.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
