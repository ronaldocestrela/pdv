using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Products;
using Pdv.Domain.Entities;

namespace Pdv.Application.Handlers.Products;

/// <summary>
/// Initializes a new instance of the <see cref="CreateProductCommandHandler"/> class.
/// </summary>
public sealed class CreateProductCommandHandler(IProductRepository products) : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _products = products;

    /// <summary>
    /// Executes the <see cref="CreateProduct"/> to perform the corresponding business action.
    /// </summary>
    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            IsActive = request.IsActive,
        };

        _products.Add(product);
        await _products.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}
