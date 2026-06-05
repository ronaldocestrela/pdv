using MediatR;
using Pdv.Modules.Reports.Application.Abstractions;

namespace Pdv.Modules.Reports.Application.Queries;

public sealed record GetTopProductsReportQuery(DateTime FromUtc, DateTime ToUtc, int Take = 20)
    : IRequest<IReadOnlyList<TopProductReportDto>>;
