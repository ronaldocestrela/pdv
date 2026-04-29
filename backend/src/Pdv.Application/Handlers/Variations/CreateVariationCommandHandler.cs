using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Variations;
using Pdv.Domain.Entities;

namespace Pdv.Application.Handlers.Variations;

public sealed class CreateVariationCommandHandler : IRequestHandler<CreateVariationCommand, int>
{
    private readonly IProductRepository _products;

    public CreateVariationCommandHandler(IProductRepository products)
    {
        _products = products;
    }

    public async Task<int> Handle(CreateVariationCommand request, CancellationToken cancellationToken)
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
