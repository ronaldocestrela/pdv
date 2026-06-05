using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Stock.Application.Abstractions;
using Pdv.Modules.Stock.Application.Commands.Stock;
using Pdv.Modules.Stock.Domain.Entities;
using Pdv.Shared.Kernel.Enums;

namespace Pdv.Modules.Stock.Application.Handlers.Stock;

/// <summary>
/// Initializes a new instance of the <see cref="AddStockCommandHandler"/> class.
/// </summary>
public sealed class AddStockCommandHandler(IStockRepository products) : IRequestHandler<AddStockCommand, Unit>
{
    private readonly IStockRepository _products = products;

    /// <summary>
    /// Executes the <see cref="AddStock"/> to perform the corresponding business action.
    /// </summary>
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
