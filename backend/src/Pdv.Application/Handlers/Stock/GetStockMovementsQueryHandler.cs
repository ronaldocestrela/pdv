using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Stock;

namespace Pdv.Application.Handlers.Stock;

public sealed class GetStockMovementsQueryHandler : IRequestHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementListItemDto>>
{
    private readonly IProductRepository _products;

    public GetStockMovementsQueryHandler(IProductRepository products)
    {
        _products = products;
    }

    public Task<IReadOnlyList<StockMovementListItemDto>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken) =>
        _products.ListStockMovementsAsync(request.ProductVariationId, request.Take, cancellationToken);
}
