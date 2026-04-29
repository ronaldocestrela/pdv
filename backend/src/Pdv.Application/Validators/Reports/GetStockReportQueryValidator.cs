using FluentValidation;
using Pdv.Application.Queries.Reports;

namespace Pdv.Application.Validators.Reports;

public sealed class GetStockReportQueryValidator : AbstractValidator<GetStockReportQuery>
{
    public GetStockReportQueryValidator()
    {
        RuleFor(x => x.Take).InclusiveBetween(1, 500);
    }
}
