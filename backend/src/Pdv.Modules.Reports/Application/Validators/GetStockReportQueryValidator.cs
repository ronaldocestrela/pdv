using FluentValidation;
using Pdv.Modules.Reports.Application.Queries;

namespace Pdv.Modules.Reports.Application.Validators;

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
