using FluentValidation;
using Pdv.Modules.Identity.Application.Commands.Users;

namespace Pdv.Modules.Identity.Application.Validators.Users;

public sealed class SetUserRolesCommandValidator : AbstractValidator<SetUserRolesCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetUserRolesCommandValidator"/> class.
    /// </summary>
    public SetUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.RoleIds).NotNull();
    }
}
