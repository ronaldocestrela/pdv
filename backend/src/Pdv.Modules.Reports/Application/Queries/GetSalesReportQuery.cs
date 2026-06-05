using MediatR;
using Pdv.Modules.Reports.Application.Abstractions;

namespace Pdv.Modules.Reports.Application.Queries;

public sealed record GetSalesReportQuery(DateTime FromUtc, DateTime ToUtc) : IRequest<SalesReportDto>;
