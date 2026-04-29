using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Products;
using Pdv.Domain.Entities;

namespace Pdv.Application.Handlers.Products;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _products;

    public CreateProductCommandHandler(IProductRepository products)
    {
        _products = products;
    }

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
