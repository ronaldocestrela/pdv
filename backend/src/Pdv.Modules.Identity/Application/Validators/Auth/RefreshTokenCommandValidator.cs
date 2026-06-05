using FluentValidation;
using Pdv.Modules.Identity.Application.Commands.Auth;

namespace Pdv.Modules.Identity.Application.Validators.Auth;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenCommandValidator"/> class.
    /// </summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
