using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Reports;

namespace Pdv.Application.Handlers.Reports;

/// <summary>
/// Initializes a new instance of the <see cref="GetTopProductsReportQueryHandler"/> class.
/// </summary>
public sealed class GetTopProductsReportQueryHandler(IReportRepository reports) : IRequestHandler<GetTopProductsReportQuery, IReadOnlyList<TopProductReportDto>>
{
    private readonly IReportRepository _reports = reports;

    /// <summary>
    /// Executes the <see cref="GetTopProductsReport"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<TopProductReportDto>> Handle(GetTopProductsReportQuery request, CancellationToken cancellationToken) =>
        _reports.GetTopProductsAsync(request.FromUtc, request.ToUtc, request.Take, cancellationToken);
}
