using MediatR;
using Pdv.Modules.Reports.Application.Abstractions;

namespace Pdv.Modules.Reports.Application.Queries;

public sealed record GetStockReportQuery(int Take = 500) : IRequest<IReadOnlyList<StockReportRowDto>>;
