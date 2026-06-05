using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Queries.Products;

namespace Pdv.Modules.Catalog.Application.Handlers.Products;

/// <summary>
/// Initializes a new instance of the <see cref="GetProductByIdQueryHandler"/> class.
/// </summary>
public sealed class GetProductByIdQueryHandler(ICatalogRepository products) : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
{
    private readonly ICatalogRepository _products = products;

    /// <summary>
    /// Executes the <see cref="GetProductById"/> to retrieve the requested data.
    /// </summary>
    public Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        _products.GetDetailByIdAsync(request.Id, cancellationToken);
}
