using MediatR;
using Pdv.Modules.Reports.Application.Abstractions;
using Pdv.Modules.Reports.Application.Queries;

namespace Pdv.Modules.Reports.Application.Handlers;

/// <summary>
/// Initializes a new instance of the <see cref="GetSalesReportQueryHandler"/> class.
/// </summary>
public sealed class GetSalesReportQueryHandler(IReportRepository reports) : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    private readonly IReportRepository _reports = reports;

    /// <summary>
    /// Executes the <see cref="GetSalesReport"/> to retrieve the requested data.
    /// </summary>
    public Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken) =>
        _reports.GetSalesSummaryAsync(request.FromUtc, request.ToUtc, cancellationToken);
}
