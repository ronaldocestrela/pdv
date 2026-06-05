using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Products;

namespace Pdv.Application.Handlers.Products;

/// <summary>
/// Initializes a new instance of the <see cref="GetProductByIdQueryHandler"/> class.
/// </summary>
public sealed class GetProductByIdQueryHandler(IProductRepository products) : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
{
    private readonly IProductRepository _products = products;

    /// <summary>
    /// Executes the <see cref="GetProductById"/> to retrieve the requested data.
    /// </summary>
    public Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        _products.GetDetailByIdAsync(request.Id, cancellationToken);
}
