using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Sales;

namespace Pdv.Application.Handlers.Sales;

public sealed class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, IReadOnlyList<SaleListItemDto>>
{
    private readonly ISaleRepository _sales;

    public GetSalesQueryHandler(ISaleRepository sales)
    {
        _sales = sales;
    }

    public Task<IReadOnlyList<SaleListItemDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken) =>
        _sales.ListRecentAsync(request.Take, cancellationToken);
}
