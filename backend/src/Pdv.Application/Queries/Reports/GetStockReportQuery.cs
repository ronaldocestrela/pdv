using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Reports;

public sealed record GetStockReportQuery(int Take = 500) : IRequest<IReadOnlyList<StockReportRowDto>>;
