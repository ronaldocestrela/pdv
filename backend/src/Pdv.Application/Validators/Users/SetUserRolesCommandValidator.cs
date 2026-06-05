using FluentValidation;
using Pdv.Application.Commands.Users;

namespace Pdv.Application.Validators.Users;

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
