using FluentValidation;
using Pdv.Modules.Identity.Application.Commands.Tenants;

namespace Pdv.Modules.Identity.Application.Validators.Tenants;

/// <summary>
/// Valida as entradas do comando <see cref="CreateTenantCommand"/> antes da execução do handler.
/// </summary>
public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    /// <summary>
    /// Inicializa as regras de validação para criação de tenant.
    /// </summary>
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
            .MaximumLength(100).WithMessage("O nome da empresa deve ter no máximo 100 caracteres.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("O e-mail do administrador é obrigatório.")
            .EmailAddress().WithMessage("Informe um e-mail válido.")
            .MaximumLength(256).WithMessage("O e-mail deve ter no máximo 256 caracteres.");

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.")
            .MaximumLength(128).WithMessage("A senha deve ter no máximo 128 caracteres.");
    }
}
