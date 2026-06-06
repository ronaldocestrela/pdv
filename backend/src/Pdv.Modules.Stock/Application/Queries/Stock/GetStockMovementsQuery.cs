using MediatR;
using Pdv.Modules.Stock.Application.Abstractions;

namespace Pdv.Modules.Stock.Application.Queries.Stock;

public sealed record GetStockMovementsQuery(Guid? ProductVariationId, int Take = 100)
    : IRequest<IReadOnlyList<StockMovementListItemDto>>;
