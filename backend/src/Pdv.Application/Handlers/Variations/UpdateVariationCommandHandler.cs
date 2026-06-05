using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Variations;

namespace Pdv.Application.Handlers.Variations;

/// <summary>
/// Initializes a new instance of the <see cref="UpdateVariationCommandHandler"/> class.
/// </summary>
public sealed class UpdateVariationCommandHandler(IProductRepository products) : IRequestHandler<UpdateVariationCommand, Unit>
{
    private readonly IProductRepository _products = products;

    /// <summary>
    /// Executes the <see cref="UpdateVariation"/> to perform the corresponding business action.
    /// </summary>
    public async Task<Unit> Handle(UpdateVariationCommand request, CancellationToken cancellationToken)
    {
        var v = await _products.GetTrackedVariationByIdAsync(request.Id, cancellationToken);
        if (v is null)
            throw new ValidationException([new ValidationFailure(nameof(request.Id), "Variação não encontrada.")]);

        var barcode = NormalizeBarcode(request.Barcode);
        if (barcode is not null && await _products.IsBarcodeTakenAsync(barcode, v.Id, cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(request.Barcode), "Código de barras já cadastrado.")]);

        v.Name = request.Name.Trim();
        v.Barcode = barcode;
        v.StockQuantity = request.StockQuantity;
        v.UnitPrice = request.UnitPrice;

        await _products.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private static string? NormalizeBarcode(string? barcode) =>
        string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
}
