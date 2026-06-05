using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Commands.Products;

namespace Pdv.Modules.Catalog.Application.Handlers.Products;

/// <summary>
/// Initializes a new instance of the <see cref="DeleteProductCommandHandler"/> class.
/// </summary>
public sealed class DeleteProductCommandHandler(ICatalogRepository products) : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly ICatalogRepository _products = products;

    /// <summary>
    /// Executes the <see cref="DeleteProduct"/> to perform the corresponding business action.
    /// </summary>
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _products.GetTrackedByIdAsync(request.Id, cancellationToken);
        if (product is null)
            throw new ValidationException([new ValidationFailure(nameof(request.Id), "Produto não encontrado.")]);

        _products.Remove(product);
        await _products.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
