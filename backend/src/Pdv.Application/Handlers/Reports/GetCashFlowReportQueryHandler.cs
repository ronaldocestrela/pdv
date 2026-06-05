using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Reports;

namespace Pdv.Application.Handlers.Reports;

/// <summary>
/// Initializes a new instance of the <see cref="GetCashFlowReportQueryHandler"/> class.
/// </summary>
public sealed class GetCashFlowReportQueryHandler(IReportRepository reports) : IRequestHandler<GetCashFlowReportQuery, IReadOnlyList<CashFlowReportRowDto>>
{
    private readonly IReportRepository _reports = reports;

    /// <summary>
    /// Executes the <see cref="GetCashFlowReport"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<CashFlowReportRowDto>> Handle(GetCashFlowReportQuery request, CancellationToken cancellationToken) =>
        _reports.ListCashFlowsAsync(request.FromUtc, request.ToUtc, request.Take, cancellationToken);
}
