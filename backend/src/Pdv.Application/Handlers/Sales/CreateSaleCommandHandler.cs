using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Sales;
using Pdv.Domain.Entities;
using Pdv.Domain.Enums;

namespace Pdv.Application.Handlers.Sales;

public sealed class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, CreateSaleResultDto>
{
    private readonly IProductRepository _products;
    private readonly ISaleRepository _sales;

    public CreateSaleCommandHandler(IProductRepository products, ISaleRepository sales)
    {
        _products = products;
        _sales = sales;
    }

    public async Task<CreateSaleResultDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var merged = MergeQuantities(request.Items);
        var now = DateTime.UtcNow;

        var resolved = new List<(ProductVariation Variation, int Quantity, decimal UnitPrice)>();
        decimal total = 0;

        foreach (var line in merged)
        {
            var variation = await _products.GetTrackedVariationByIdAsync(line.ProductVariationId, cancellationToken);
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

        foreach (var (variation, qty, unitPrice) in resolved)
        {
            variation.StockQuantity -= qty;

            _sales.AddItem(new SaleItem
            {
                Sale = sale,
                ProductVariationId = variation.Id,
                Quantity = qty,
                UnitPrice = unitPrice,
            });

            _products.AddStockMovement(new StockMovement
            {
                ProductVariationId = variation.Id,
                Type = StockMovementType.Out,
                Quantity = qty,
                CreatedAtUtc = now,
                Reason = null,
            });
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

        await _products.SaveChangesAsync(cancellationToken);

        return new CreateSaleResultDto(sale.Id, total);
    }

    private static List<(int ProductVariationId, int Quantity)> MergeQuantities(IReadOnlyList<CreateSaleLineDto> items)
    {
        var map = new Dictionary<int, int>();
        foreach (var i in items)
            map[i.ProductVariationId] = map.GetValueOrDefault(i.ProductVariationId, 0) + i.Quantity;

        return map.Select(kv => (kv.Key, kv.Value)).ToList();
    }
}
