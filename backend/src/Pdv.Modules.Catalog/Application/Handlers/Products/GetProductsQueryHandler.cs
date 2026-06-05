using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Queries.Products;

namespace Pdv.Modules.Catalog.Application.Handlers.Products;

/// <summary>
/// Initializes a new instance of the <see cref="GetProductsQueryHandler"/> class.
/// </summary>
public sealed class GetProductsQueryHandler(ICatalogRepository products) : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductSummaryDto>>
{
    private readonly ICatalogRepository _products = products;

    /// <summary>
    /// Executes the <see cref="GetProducts"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<ProductSummaryDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        _products.ListSummariesAsync(cancellationToken);
}
