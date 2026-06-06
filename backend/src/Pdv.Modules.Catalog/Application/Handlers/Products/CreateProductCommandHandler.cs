using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Commands.Products;
using Pdv.Modules.Catalog.Domain.Entities;

namespace Pdv.Modules.Catalog.Application.Handlers.Products;

/// <summary>
/// Initializes a new instance of the <see cref="CreateProductCommandHandler"/> class.
/// </summary>
public sealed class CreateProductCommandHandler(ICatalogRepository products) : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly ICatalogRepository _products = products;

    /// <summary>
    /// Executes the <see cref="CreateProduct"/> to perform the corresponding business action.
    /// </summary>
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
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
