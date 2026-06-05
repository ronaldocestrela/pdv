using MediatR;
using Pdv.Modules.Stock.Application.Abstractions;
using Pdv.Modules.Stock.Application.Queries.Stock;

namespace Pdv.Modules.Stock.Application.Handlers.Stock;

/// <summary>
/// Initializes a new instance of the <see cref="GetStockMovementsQueryHandler"/> class.
/// </summary>
public sealed class GetStockMovementsQueryHandler(IStockRepository products) : IRequestHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementListItemDto>>
{
    private readonly IStockRepository _products = products;

    /// <summary>
    /// Executes the <see cref="GetStockMovements"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<StockMovementListItemDto>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken) =>
        _products.ListStockMovementsAsync(request.ProductVariationId, request.Take, cancellationToken);
}
