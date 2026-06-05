using MediatR;
using Pdv.Modules.Sales.Application.Abstractions;

namespace Pdv.Modules.Sales.Application.Queries.Sales;

public sealed record GetSalesQuery(int Take = 100) : IRequest<IReadOnlyList<SaleListItemDto>>;
