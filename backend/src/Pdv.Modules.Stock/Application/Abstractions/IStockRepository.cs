using Pdv.Modules.Stock.Domain.Entities;

namespace Pdv.Modules.Stock.Application.Abstractions;

public sealed record StockMovementListItemDto(
    Guid Id,
    Guid ProductVariationId,
    string ProductName,
    string VariationName,
    string Type,
    int Quantity,
    DateTime CreatedAtUtc,
    string? Reason);

public interface IStockRepository
{
    Task<IReadOnlyList<StockMovementListItemDto>> ListStockMovementsAsync(
        Guid? productVariationId,
        int take,
        CancellationToken cancellationToken = default);

    void AddStockMovement(StockMovement movement);

    Task<ProductVariation?> GetTrackedVariationByIdAsync(Guid variationId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
