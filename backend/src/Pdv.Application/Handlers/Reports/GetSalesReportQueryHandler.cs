using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Reports;

namespace Pdv.Application.Handlers.Reports;

public sealed class GetSalesReportQueryHandler : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    private readonly IReportRepository _reports;

    public GetSalesReportQueryHandler(IReportRepository reports)
    {
        _reports = reports;
    }

    public Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken) =>
        _reports.GetSalesSummaryAsync(request.FromUtc, request.ToUtc, cancellationToken);
}
