using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Stock;
using Pdv.Domain.Entities;
using Pdv.Domain.Enums;

namespace Pdv.Application.Handlers.Stock;

public sealed class AddStockCommandHandler : IRequestHandler<AddStockCommand, Unit>
{
    private readonly IProductRepository _products;

    public AddStockCommandHandler(IProductRepository products)
    {
        _products = products;
    }

    public async Task<Unit> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        var v = await _products.GetTrackedVariationByIdAsync(request.ProductVariationId, cancellationToken);
        if (v is null)
            throw new ValidationException([new ValidationFailure(nameof(request.ProductVariationId), "Variação não encontrada.")]);

        var newQty = (long)v.StockQuantity + request.Quantity;
        if (newQty > int.MaxValue)
            throw new ValidationException([new ValidationFailure(nameof(request.Quantity), "Quantidade excede o limite do estoque.")]);

        v.StockQuantity = (int)newQty;

        var movement = new StockMovement
        {
            ProductVariationId = v.Id,
            Type = StockMovementType.In,
            Quantity = request.Quantity,
            CreatedAtUtc = DateTime.UtcNow,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
        };

        _products.AddStockMovement(movement);
        await _products.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
