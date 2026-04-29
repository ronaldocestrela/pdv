using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Reports;

public sealed record GetCashFlowReportQuery(DateTime FromUtc, DateTime ToUtc, int Take = 100)
    : IRequest<IReadOnlyList<CashFlowReportRowDto>>;
