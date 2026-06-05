using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Stock;

namespace Pdv.Application.Handlers.Stock;

/// <summary>
/// Initializes a new instance of the <see cref="GetStockMovementsQueryHandler"/> class.
/// </summary>
public sealed class GetStockMovementsQueryHandler(IProductRepository products) : IRequestHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementListItemDto>>
{
    private readonly IProductRepository _products = products;

    /// <summary>
    /// Executes the <see cref="GetStockMovements"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<StockMovementListItemDto>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken) =>
        _products.ListStockMovementsAsync(request.ProductVariationId, request.Take, cancellationToken);
}
