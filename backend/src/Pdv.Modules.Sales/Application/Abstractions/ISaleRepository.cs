using Pdv.Modules.Sales.Domain.Entities;
using Pdv.Shared.Kernel.Enums;

namespace Pdv.Modules.Sales.Application.Abstractions;

public sealed record SaleListItemDto(
    Guid Id,
    DateTime CreatedAtUtc,
    decimal TotalAmount,
    PaymentMethod PaymentMethod,
    int ItemCount);

public interface ISaleRepository
{
    void Add(Sale sale);

    void AddItem(SaleItem item);

    void AddCashFlow(CashFlow cashFlow);

    Task<IReadOnlyList<SaleListItemDto>> ListRecentAsync(int take, CancellationToken cancellationToken = default);

    Task<ProductVariation?> GetVariationByIdAsync(Guid variationId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
