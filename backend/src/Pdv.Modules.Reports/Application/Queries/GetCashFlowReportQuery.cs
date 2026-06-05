using MediatR;
using Pdv.Modules.Reports.Application.Abstractions;

namespace Pdv.Modules.Reports.Application.Queries;

public sealed record GetCashFlowReportQuery(DateTime FromUtc, DateTime ToUtc, int Take = 100)
    : IRequest<IReadOnlyList<CashFlowReportRowDto>>;
