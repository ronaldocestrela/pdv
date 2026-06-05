using MediatR;
using Pdv.Modules.Reports.Application.Abstractions;
using Pdv.Modules.Reports.Application.Queries;

namespace Pdv.Modules.Reports.Application.Handlers;

/// <summary>
/// Initializes a new instance of the <see cref="GetStockReportQueryHandler"/> class.
/// </summary>
public sealed class GetStockReportQueryHandler(IReportRepository reports) : IRequestHandler<GetStockReportQuery, IReadOnlyList<StockReportRowDto>>
{
    private readonly IReportRepository _reports = reports;

    /// <summary>
    /// Executes the <see cref="GetStockReport"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<StockReportRowDto>> Handle(GetStockReportQuery request, CancellationToken cancellationToken) =>
        _reports.ListStockLevelsAsync(request.Take, cancellationToken);
}
