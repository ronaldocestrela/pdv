using FluentValidation;
using Pdv.Application.Commands.Roles;

namespace Pdv.Application.Validators.Roles;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
