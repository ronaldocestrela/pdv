using MediatR;

namespace Pdv.Shared.Kernel.Events;

public sealed record SaleFinalizedItemDto(int ProductVariationId, int Quantity);

public sealed record SaleFinalizedIntegrationEvent(
    int SaleId,
    int TenantId,
    IReadOnlyList<SaleFinalizedItemDto> Items,
    DateTime CreatedAtUtc) : INotification;
