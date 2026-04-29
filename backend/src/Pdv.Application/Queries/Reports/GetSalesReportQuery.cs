using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Reports;

public sealed record GetSalesReportQuery(DateTime FromUtc, DateTime ToUtc) : IRequest<SalesReportDto>;
