using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Products;

namespace Pdv.Application.Handlers.Products;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductSummaryDto>>
{
    private readonly IProductRepository _products;

    public GetProductsQueryHandler(IProductRepository products)
    {
        _products = products;
    }

    public Task<IReadOnlyList<ProductSummaryDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        _products.ListSummariesAsync(cancellationToken);
}
