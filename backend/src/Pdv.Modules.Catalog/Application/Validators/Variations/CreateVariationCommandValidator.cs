using FluentValidation;
using Pdv.Modules.Catalog.Application.Commands.Variations;

namespace Pdv.Modules.Catalog.Application.Validators.Variations;

public sealed class CreateVariationCommandValidator : AbstractValidator<CreateVariationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateVariationCommandValidator"/> class.
    /// </summary>
    public CreateVariationCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Preço unitário não pode ser negativo.");
        RuleFor(x => x.Barcode).MaximumLength(64).Unless(x => string.IsNullOrWhiteSpace(x.Barcode));
    }
}
