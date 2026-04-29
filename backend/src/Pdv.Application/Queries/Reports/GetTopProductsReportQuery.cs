using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Reports;

public sealed record GetTopProductsReportQuery(DateTime FromUtc, DateTime ToUtc, int Take = 20)
    : IRequest<IReadOnlyList<TopProductReportDto>>;
