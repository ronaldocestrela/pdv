using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Stock;

public sealed record GetStockMovementsQuery(int? ProductVariationId, int Take = 100)
    : IRequest<IReadOnlyList<StockMovementListItemDto>>;
