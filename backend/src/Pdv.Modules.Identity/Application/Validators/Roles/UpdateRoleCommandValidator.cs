using FluentValidation;
using Pdv.Modules.Identity.Application.Commands.Roles;

namespace Pdv.Modules.Identity.Application.Validators.Roles;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleCommandValidator"/> class.
    /// </summary>
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
