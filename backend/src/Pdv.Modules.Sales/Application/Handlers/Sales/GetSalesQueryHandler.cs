using MediatR;
using Pdv.Modules.Sales.Application.Abstractions;
using Pdv.Modules.Sales.Application.Queries.Sales;

namespace Pdv.Modules.Sales.Application.Handlers.Sales;

/// <summary>
/// Initializes a new instance of the <see cref="GetSalesQueryHandler"/> class.
/// </summary>
public sealed class GetSalesQueryHandler(ISaleRepository sales) : IRequestHandler<GetSalesQuery, IReadOnlyList<SaleListItemDto>>
{
    private readonly ISaleRepository _sales = sales;

    /// <summary>
    /// Executes the <see cref="GetSales"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<SaleListItemDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken) =>
        _sales.ListRecentAsync(request.Take, cancellationToken);
}
