using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Sales.Application.Abstractions;
using Pdv.Modules.Sales.Application.Commands.Sales;
using Pdv.Modules.Sales.Domain.Entities;
using Pdv.Shared.Kernel.Enums;
using Pdv.Shared.Kernel.Events;

namespace Pdv.Modules.Sales.Application.Handlers.Sales;

/// <summary>
/// Handler para o comando de criação de venda. Registra a venda e publica o evento de finalização.
/// </summary>
public sealed class CreateSaleCommandHandler(
    ISaleRepository sales,
    IPublisher publisher) : IRequestHandler<CreateSaleCommand, CreateSaleResultDto>
{
    private readonly ISaleRepository _sales = sales;
    private readonly IPublisher _publisher = publisher;

    public async Task<CreateSaleResultDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var merged = MergeQuantities(request.Items);
        var now = DateTime.UtcNow;

        var resolved = new List<(ProductVariation Variation, int Quantity, decimal UnitPrice)>();
        decimal total = 0;

        foreach (var line in merged)
        {
            var variation = await _sales.GetVariationByIdAsync(line.ProductVariationId, cancellationToken);
            if (variation is null)
            {
                throw new ValidationException([
                    new ValidationFailure(nameof(request.Items), $"Variação {line.ProductVariationId} não encontrada."),
                ]);
            }

            if (variation.StockQuantity < line.Quantity)
            {
                throw new ValidationException([
                    new ValidationFailure(
                        nameof(request.Items),
                        $"Estoque insuficiente para '{variation.Name}' (disponível: {variation.StockQuantity}, solicitado: {line.Quantity})."),
                ]);
            }

            var unitPrice = variation.UnitPrice;
            total += line.Quantity * unitPrice;
            resolved.Add((variation, line.Quantity, unitPrice));
        }

        var sale = new Sale
        {
            CreatedAtUtc = now,
            TotalAmount = total,
            PaymentMethod = request.PaymentMethod,
        };

        _sales.Add(sale);

        var integrationItems = new List<SaleFinalizedItemDto>();

        foreach (var (variation, qty, unitPrice) in resolved)
        {
            _sales.AddItem(new SaleItem
            {
                Sale = sale,
                ProductVariationId = variation.Id,
                Quantity = qty,
                UnitPrice = unitPrice,
            });

            integrationItems.Add(new SaleFinalizedItemDto(variation.Id, qty));
        }

        var paymentLabel = request.PaymentMethod switch
        {
            PaymentMethod.Cash => "Dinheiro",
            PaymentMethod.Card => "Cartão",
            PaymentMethod.Pix => "PIX",
            _ => request.PaymentMethod.ToString(),
        };

        _sales.AddCashFlow(new CashFlow
        {
            Type = CashFlowType.In,
            Amount = total,
            Description = $"Entrada venda PDV ({paymentLabel})",
            CreatedAtUtc = now,
            Sale = sale,
        });

        await _sales.SaveChangesAsync(cancellationToken);

        // Publica o evento de integração para debitar estoque/criar movimentos reativamente no módulo Stock
        await _publisher.Publish(new SaleFinalizedIntegrationEvent(
            sale.Id,
            sale.TenantId,
            integrationItems,
            now), cancellationToken);

        return new CreateSaleResultDto(sale.Id, total);
    }

    private static List<(Guid ProductVariationId, int Quantity)> MergeQuantities(IReadOnlyList<CreateSaleLineDto> items)
    {
        var map = new Dictionary<Guid, int>();
        foreach (var i in items)
            map[i.ProductVariationId] = map.GetValueOrDefault(i.ProductVariationId, 0) + i.Quantity;

        return map.Select(kv => (kv.Key, kv.Value)).ToList();
    }
}
