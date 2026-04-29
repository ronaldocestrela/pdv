using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Reports;

namespace Pdv.Application.Handlers.Reports;

public sealed class GetStockReportQueryHandler : IRequestHandler<GetStockReportQuery, IReadOnlyList<StockReportRowDto>>
{
    private readonly IReportRepository _reports;

    public GetStockReportQueryHandler(IReportRepository reports)
    {
        _reports = reports;
    }

    public Task<IReadOnlyList<StockReportRowDto>> Handle(GetStockReportQuery request, CancellationToken cancellationToken) =>
        _reports.ListStockLevelsAsync(request.Take, cancellationToken);
}
