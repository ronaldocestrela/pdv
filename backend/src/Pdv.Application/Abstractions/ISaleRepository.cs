using Pdv.Domain.Entities;
using Pdv.Domain.Enums;

namespace Pdv.Application.Abstractions;

public sealed record SaleListItemDto(
    int Id,
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
}
