using FluentValidation;
using Pdv.Application.Commands.Auth;

namespace Pdv.Application.Validators.Auth;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
