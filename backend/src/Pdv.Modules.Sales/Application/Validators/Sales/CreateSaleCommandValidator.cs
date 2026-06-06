using FluentValidation;
using Pdv.Modules.Sales.Application.Commands.Sales;

namespace Pdv.Modules.Sales.Application.Validators.Sales;

public sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSaleCommandValidator"/> class.
    /// </summary>
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Informe ao menos um item na venda.");

        RuleForEach(x => x.Items).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductVariationId)
                .NotEmpty()
                .WithMessage("Variação inválida.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantidade deve ser maior que zero.");
        });
    }
}
