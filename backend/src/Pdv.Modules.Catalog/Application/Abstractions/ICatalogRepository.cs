using Pdv.Modules.Catalog.Domain.Entities;

namespace Pdv.Modules.Catalog.Application.Abstractions;

public sealed record ProductSummaryDto(int Id, string Name, bool IsActive, int VariationCount);

public sealed record ProductVariationDto(int Id, int ProductId, string Name, string? Barcode, int StockQuantity, decimal UnitPrice);

public sealed record ProductDetailDto(int Id, string Name, bool IsActive, IReadOnlyList<ProductVariationDto> Variations);

public interface ICatalogRepository
{
    Task<IReadOnlyList<ProductSummaryDto>> ListSummariesAsync(CancellationToken cancellationToken = default);

    Task<ProductDetailDto?> GetDetailByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default);

    Task<Product?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default);

    void Add(Product product);

    void Remove(Product product);

    Task<ProductVariation?> GetTrackedVariationByIdAsync(int variationId, CancellationToken cancellationToken = default);

    void AddVariation(ProductVariation variation);

    void RemoveVariation(ProductVariation variation);

    /// <summary>Returns true if another variation already uses this barcode.</summary>
    Task<bool> IsBarcodeTakenAsync(string barcode, int? excludeVariationId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
