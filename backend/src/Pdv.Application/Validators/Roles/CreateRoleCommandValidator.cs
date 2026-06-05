using FluentValidation;
using Pdv.Application.Commands.Roles;

namespace Pdv.Application.Validators.Roles;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateRoleCommandValidator"/> class.
    /// </summary>
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
