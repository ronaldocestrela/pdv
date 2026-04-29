using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Products;

namespace Pdv.Application.Handlers.Products;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
{
    private readonly IProductRepository _products;

    public GetProductByIdQueryHandler(IProductRepository products)
    {
        _products = products;
    }

    public Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        _products.GetDetailByIdAsync(request.Id, cancellationToken);
}
