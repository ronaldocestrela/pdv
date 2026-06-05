using FluentValidation;
using Pdv.Application.Commands.Stock;

namespace Pdv.Application.Validators.Stock;

public sealed class AddStockCommandValidator : AbstractValidator<AddStockCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddStockCommandValidator"/> class.
    /// </summary>
    public AddStockCommandValidator()
    {
        RuleFor(x => x.ProductVariationId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason)
            .MaximumLength(512)
            .When(x => x.Reason is not null);
    }
}
