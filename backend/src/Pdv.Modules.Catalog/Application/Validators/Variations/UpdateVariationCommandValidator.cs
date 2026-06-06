using FluentValidation;
using Pdv.Modules.Catalog.Application.Commands.Variations;

namespace Pdv.Modules.Catalog.Application.Validators.Variations;

public sealed class UpdateVariationCommandValidator : AbstractValidator<UpdateVariationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateVariationCommandValidator"/> class.
    /// </summary>
    public UpdateVariationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Preço unitário não pode ser negativo.");
        RuleFor(x => x.Barcode).MaximumLength(64).Unless(x => string.IsNullOrWhiteSpace(x.Barcode));
    }
}
