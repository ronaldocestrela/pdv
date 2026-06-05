using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Commands.Products;

namespace Pdv.Modules.Catalog.Application.Handlers.Products;

/// <summary>
/// Initializes a new instance of the <see cref="UpdateProductCommandHandler"/> class.
/// </summary>
public sealed class UpdateProductCommandHandler(ICatalogRepository products) : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly ICatalogRepository _products = products;

    /// <summary>
    /// Executes the <see cref="UpdateProduct"/> to perform the corresponding business action.
    /// </summary>
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _products.GetTrackedByIdAsync(request.Id, cancellationToken);
        if (product is null)
            throw new ValidationException([new ValidationFailure(nameof(request.Id), "Produto não encontrado.")]);

        product.Name = request.Name.Trim();
        product.IsActive = request.IsActive;
        await _products.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
