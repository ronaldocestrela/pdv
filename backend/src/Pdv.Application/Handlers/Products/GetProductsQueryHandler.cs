using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Products;

namespace Pdv.Application.Handlers.Products;

/// <summary>
/// Initializes a new instance of the <see cref="GetProductsQueryHandler"/> class.
/// </summary>
public sealed class GetProductsQueryHandler(IProductRepository products) : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductSummaryDto>>
{
    private readonly IProductRepository _products = products;

    /// <summary>
    /// Executes the <see cref="GetProducts"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<ProductSummaryDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        _products.ListSummariesAsync(cancellationToken);
}
