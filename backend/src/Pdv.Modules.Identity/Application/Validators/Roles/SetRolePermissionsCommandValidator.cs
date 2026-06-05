using FluentValidation;
using Pdv.Modules.Identity.Application.Commands.Roles;

namespace Pdv.Modules.Identity.Application.Validators.Roles;

public sealed class SetRolePermissionsCommandValidator : AbstractValidator<SetRolePermissionsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetRolePermissionsCommandValidator"/> class.
    /// </summary>
    public SetRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThan(0);
        RuleFor(x => x.PermissionNames).NotNull();
        RuleForEach(x => x.PermissionNames).NotEmpty().MaximumLength(128);
    }
}
