using MediatR;
using Pdv.Modules.Stock.Application.Abstractions;
using Pdv.Modules.Stock.Domain.Entities;
using Pdv.Shared.Kernel.Events;
using Pdv.Shared.Kernel.Enums;

namespace Pdv.Modules.Stock.Application.Handlers.Integration;

public sealed class DecrementStockOnSaleFinalized(IStockRepository products) : INotificationHandler<SaleFinalizedIntegrationEvent>
{
    private readonly IStockRepository _products = products;

    public async Task Handle(SaleFinalizedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        foreach (var item in notification.Items)
        {
            var variation = await _products.GetTrackedVariationByIdAsync(item.ProductVariationId, cancellationToken);
            if (variation is null)
                continue; // Ou joga erro dependendo da necessidade, mas para o MVP vamos apenas pular ou logar.

            variation.StockQuantity -= item.Quantity;

            _products.AddStockMovement(new StockMovement
            {
                ProductVariationId = variation.Id,
                Type = StockMovementType.Out,
                Quantity = item.Quantity,
                CreatedAtUtc = notification.CreatedAtUtc,
                Reason = $"Baixa automática (Venda #{notification.SaleId})"
            });
        }

        await _products.SaveChangesAsync(cancellationToken);
    }
}
