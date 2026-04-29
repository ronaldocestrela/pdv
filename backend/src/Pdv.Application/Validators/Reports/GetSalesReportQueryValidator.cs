using FluentValidation;
using Pdv.Application.Queries.Reports;

namespace Pdv.Application.Validators.Reports;

public sealed class GetSalesReportQueryValidator : AbstractValidator<GetSalesReportQuery>
{
    public GetSalesReportQueryValidator()
    {
        RuleFor(x => x.ToUtc).GreaterThanOrEqualTo(x => x.FromUtc).WithMessage("toUtc must be >= fromUtc.");

        RuleFor(x => x).Must(x => (x.ToUtc - x.FromUtc).TotalDays <= 366)
            .WithMessage("Date range cannot exceed 366 days.");
    }
}
