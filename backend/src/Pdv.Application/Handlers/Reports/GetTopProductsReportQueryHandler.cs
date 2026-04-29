using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Reports;

namespace Pdv.Application.Handlers.Reports;

public sealed class GetTopProductsReportQueryHandler : IRequestHandler<GetTopProductsReportQuery, IReadOnlyList<TopProductReportDto>>
{
    private readonly IReportRepository _reports;

    public GetTopProductsReportQueryHandler(IReportRepository reports)
    {
        _reports = reports;
    }

    public Task<IReadOnlyList<TopProductReportDto>> Handle(GetTopProductsReportQuery request, CancellationToken cancellationToken) =>
        _reports.GetTopProductsAsync(request.FromUtc, request.ToUtc, request.Take, cancellationToken);
}
