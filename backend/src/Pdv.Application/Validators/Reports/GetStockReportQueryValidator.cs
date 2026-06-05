using FluentValidation;
using Pdv.Application.Queries.Reports;

namespace Pdv.Application.Validators.Reports;

public sealed class GetStockReportQueryValidator : AbstractValidator<GetStockReportQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetStockReportQueryValidator"/> class.
    /// </summary>
    public GetStockReportQueryValidator()
    {
        RuleFor(x => x.Take).InclusiveBetween(1, 500);
    }
}
