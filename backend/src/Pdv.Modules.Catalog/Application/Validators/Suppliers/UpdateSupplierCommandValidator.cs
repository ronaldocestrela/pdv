using FluentValidation;
using Pdv.Modules.Catalog.Application.Commands.Suppliers;

namespace Pdv.Modules.Catalog.Application.Validators.Suppliers;

/// <summary>
/// Validator for the <see cref="UpdateSupplierCommand"/> class.
/// </summary>
public sealed class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSupplierCommandValidator"/> class.
    /// </summary>
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID do fornecedor é obrigatório.");

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
