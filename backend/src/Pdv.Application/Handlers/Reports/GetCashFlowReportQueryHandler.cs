using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Reports;

namespace Pdv.Application.Handlers.Reports;

public sealed class GetCashFlowReportQueryHandler : IRequestHandler<GetCashFlowReportQuery, IReadOnlyList<CashFlowReportRowDto>>
{
    private readonly IReportRepository _reports;

    public GetCashFlowReportQueryHandler(IReportRepository reports)
    {
        _reports = reports;
    }

    public Task<IReadOnlyList<CashFlowReportRowDto>> Handle(GetCashFlowReportQuery request, CancellationToken cancellationToken) =>
        _reports.ListCashFlowsAsync(request.FromUtc, request.ToUtc, request.Take, cancellationToken);
}
