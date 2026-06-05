using FluentValidation;
using Pdv.Modules.Identity.Application.Commands.Roles;

namespace Pdv.Modules.Identity.Application.Validators.Roles;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleCommandValidator"/> class.
    /// </summary>
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
