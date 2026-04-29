using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Sales;

public sealed record GetSalesQuery(int Take = 100) : IRequest<IReadOnlyList<SaleListItemDto>>;
