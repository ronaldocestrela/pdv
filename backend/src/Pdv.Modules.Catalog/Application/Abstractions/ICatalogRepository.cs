using Pdv.Modules.Catalog.Domain.Entities;

namespace Pdv.Modules.Catalog.Application.Abstractions;

public sealed record ProductSummaryDto(Guid Id, string Name, bool IsActive, int VariationCount);

public sealed record ProductVariationDto(Guid Id, Guid ProductId, string Name, string? Barcode, int StockQuantity, decimal UnitPrice);

public sealed record ProductDetailDto(Guid Id, string Name, bool IsActive, IReadOnlyList<ProductVariationDto> Variations);

/// <summary>
/// DTO representing a summary of a Supplier.
/// </summary>
public sealed record SupplierSummaryDto(Guid Id, string Name, string? Document, string? Email, string? Phone, bool IsActive);

public interface ICatalogRepository
{
    Task<IReadOnlyList<ProductSummaryDto>> ListSummariesAsync(CancellationToken cancellationToken = default);

    Task<ProductDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<Product?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(Product product);

    void Remove(Product product);

    Task<ProductVariation?> GetTrackedVariationByIdAsync(Guid variationId, CancellationToken cancellationToken = default);

    void AddVariation(ProductVariation variation);

    void RemoveVariation(ProductVariation variation);

    /// <summary>Returns true if another variation already uses this barcode.</summary>
    Task<bool> IsBarcodeTakenAsync(string barcode, Guid? excludeVariationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of DTO summaries of all active/inactive suppliers.
    /// </summary>
    Task<IReadOnlyList<SupplierSummaryDto>> ListSuppliersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a supplier by its ID, tracking it for edits.
    /// </summary>
    Task<Supplier?> GetTrackedSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a supplier's detail DTO by its ID.
    /// </summary>
    Task<SupplierSummaryDto?> GetSupplierDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new supplier.
    /// </summary>
    void AddSupplier(Supplier supplier);

    /// <summary>
    /// Removes a supplier.
    /// </summary>
    void RemoveSupplier(Supplier supplier);

    /// <summary>
    /// Checks if a document (CNPJ/CPF) is already taken by another supplier in the same tenant.
    /// </summary>
    Task<bool> IsSupplierDocumentTakenAsync(string document, Guid? excludeSupplierId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
