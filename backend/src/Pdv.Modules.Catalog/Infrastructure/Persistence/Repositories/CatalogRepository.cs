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
    public async Task<ProductDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
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
    public Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default) =>
        _db.Products.AsNoTracking().AnyAsync(p => p.Id == productId, cancellationToken);

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public Task<Product?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
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
    public Task<ProductVariation?> GetTrackedVariationByIdAsync(Guid variationId, CancellationToken cancellationToken = default)
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
    public async Task<bool> IsBarcodeTakenAsync(string barcode, Guid? excludeVariationId, CancellationToken cancellationToken = default)
    {
        var trimmed = barcode.Trim();
        var q = _db.ProductVariations.Where(v => v.Barcode != null && v.Barcode == trimmed);
        if (excludeVariationId.HasValue)
            q = q.Where(v => v.Id != excludeVariationId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a list of DTO summaries of all active/inactive suppliers.
    /// </summary>
    public async Task<IReadOnlyList<SupplierSummaryDto>> ListSuppliersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Suppliers.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SupplierSummaryDto(s.Id, s.Name, s.Document, s.Email, s.Phone, s.IsActive))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a supplier by its ID, tracking it for edits.
    /// </summary>
    public Task<Supplier?> GetTrackedSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets a supplier's detail DTO by its ID.
    /// </summary>
    public async Task<SupplierSummaryDto?> GetSupplierDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Suppliers.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SupplierSummaryDto(s.Id, s.Name, s.Document, s.Email, s.Phone, s.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new supplier.
    /// </summary>
    public void AddSupplier(Supplier supplier) =>
        _db.Suppliers.Add(supplier);

    /// <summary>
    /// Removes a supplier.
    /// </summary>
    public void RemoveSupplier(Supplier supplier) =>
        _db.Suppliers.Remove(supplier);

    /// <summary>
    /// Checks if a document (CNPJ/CPF) is already taken by another supplier in the same tenant.
    /// </summary>
    public async Task<bool> IsSupplierDocumentTakenAsync(string document, Guid? excludeSupplierId, CancellationToken cancellationToken = default)
    {
        var trimmed = document.Trim();
        var q = _db.Suppliers.Where(s => s.Document != null && s.Document == trimmed);
        if (excludeSupplierId.HasValue)
            q = q.Where(s => s.Id != excludeSupplierId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Persists all tracked changes in this database context.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
