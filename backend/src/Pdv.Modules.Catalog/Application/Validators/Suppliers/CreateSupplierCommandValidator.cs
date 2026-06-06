using FluentValidation;
using Pdv.Modules.Catalog.Application.Commands.Suppliers;

namespace Pdv.Modules.Catalog.Application.Validators.Suppliers;

/// <summary>
/// Validator for the <see cref="CreateSupplierCommand"/> class.
/// </summary>
public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSupplierCommandValidator"/> class.
    /// </summary>
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(256).WithMessage("O nome deve ter no máximo 256 caracteres.");

        RuleFor(x => x.Document)
            .MaximumLength(32).WithMessage("O documento deve ter no máximo 32 caracteres.")
            .Unless(x => string.IsNullOrWhiteSpace(x.Document));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("O e-mail informado não é válido.")
            .MaximumLength(256).WithMessage("O e-mail deve ter no máximo 256 caracteres.")
            .Unless(x => string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("O telefone deve ter no máximo 20 caracteres.")
            .Unless(x => string.IsNullOrWhiteSpace(x.Phone));
    }
}
