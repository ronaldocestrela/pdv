using FluentValidation;
using Pdv.Application.Commands.Variations;

namespace Pdv.Application.Validators.Variations;

public sealed class CreateVariationCommandValidator : AbstractValidator<CreateVariationCommand>
{
    public CreateVariationCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Preço unitário não pode ser negativo.");
        RuleFor(x => x.Barcode).MaximumLength(64).Unless(x => string.IsNullOrWhiteSpace(x.Barcode));
    }
}
