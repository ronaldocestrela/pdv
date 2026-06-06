using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Commands.Variations;
using Pdv.Modules.Catalog.Domain.Entities;

namespace Pdv.Modules.Catalog.Application.Handlers.Variations;

/// <summary>
/// Initializes a new instance of the <see cref="CreateVariationCommandHandler"/> class.
/// </summary>
public sealed class CreateVariationCommandHandler(ICatalogRepository products) : IRequestHandler<CreateVariationCommand, Guid>
{
    private readonly ICatalogRepository _products = products;

    /// <summary>
    /// Executes the <see cref="CreateVariation"/> to perform the corresponding business action.
    /// </summary>
    public async Task<Guid> Handle(CreateVariationCommand request, CancellationToken cancellationToken)
    {
        if (!await _products.ProductExistsAsync(request.ProductId, cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(request.ProductId), "Produto não encontrado.")]);

        var barcode = NormalizeBarcode(request.Barcode);
        if (barcode is not null && await _products.IsBarcodeTakenAsync(barcode, null, cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(request.Barcode), "Código de barras já cadastrado.")]);

        var v = new ProductVariation
        {
            ProductId = request.ProductId,
            Name = request.Name.Trim(),
            Barcode = barcode,
            StockQuantity = request.StockQuantity,
            UnitPrice = request.UnitPrice,
        };

        _products.AddVariation(v);
        await _products.SaveChangesAsync(cancellationToken);
        return v.Id;
    }

    private static string? NormalizeBarcode(string? barcode) =>
        string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
}
