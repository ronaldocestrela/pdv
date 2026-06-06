using FluentValidation;
using Pdv.Modules.Catalog.Application.Commands.Suppliers;

namespace Pdv.Modules.Catalog.Application.Validators.Suppliers;

/// <summary>
/// Validator for the <see cref="DeleteSupplierCommand"/> class.
/// </summary>
public sealed class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteSupplierCommandValidator"/> class.
    /// </summary>
    public DeleteSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID do fornecedor é obrigatório.");
    }
}
