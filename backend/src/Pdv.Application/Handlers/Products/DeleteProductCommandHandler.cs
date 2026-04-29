using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Products;

namespace Pdv.Application.Handlers.Products;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IProductRepository _products;

    public DeleteProductCommandHandler(IProductRepository products)
    {
        _products = products;
    }

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
