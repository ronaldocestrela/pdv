using MediatR;

namespace Pdv.Shared.Kernel.Events;

public sealed record SaleFinalizedItemDto(Guid ProductVariationId, int Quantity);

public sealed record SaleFinalizedIntegrationEvent(
    Guid SaleId,
    Guid TenantId,
    IReadOnlyList<SaleFinalizedItemDto> Items,
    DateTime CreatedAtUtc) : INotification;
