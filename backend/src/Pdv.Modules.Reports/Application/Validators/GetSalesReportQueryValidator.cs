using FluentValidation;
using Pdv.Modules.Reports.Application.Queries;

namespace Pdv.Modules.Reports.Application.Validators;

public sealed class GetSalesReportQueryValidator : AbstractValidator<GetSalesReportQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSalesReportQueryValidator"/> class.
    /// </summary>
    public GetSalesReportQueryValidator()
    {
        RuleFor(x => x.ToUtc).GreaterThanOrEqualTo(x => x.FromUtc).WithMessage("toUtc must be >= fromUtc.");

        RuleFor(x => x).Must(x => (x.ToUtc - x.FromUtc).TotalDays <= 366)
            .WithMessage("Date range cannot exceed 366 days.");
    }
}
