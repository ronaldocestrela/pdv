using FluentValidation;
using Pdv.Application.Commands.Roles;

namespace Pdv.Application.Validators.Roles;

public sealed class SetRolePermissionsCommandValidator : AbstractValidator<SetRolePermissionsCommand>
{
    public SetRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThan(0);
        RuleFor(x => x.PermissionNames).NotNull();
        RuleForEach(x => x.PermissionNames).NotEmpty().MaximumLength(128);
    }
}
