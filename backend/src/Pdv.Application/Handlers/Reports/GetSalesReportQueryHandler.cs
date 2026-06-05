using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Reports;

namespace Pdv.Application.Handlers.Reports;

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
