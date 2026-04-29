using FluentValidation;
using Pdv.Application.Commands.Stock;

namespace Pdv.Application.Validators.Stock;

public sealed class AddStockCommandValidator : AbstractValidator<AddStockCommand>
{
    public AddStockCommandValidator()
    {
        RuleFor(x => x.ProductVariationId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason)
            .MaximumLength(512)
            .When(x => x.Reason is not null);
    }
}
