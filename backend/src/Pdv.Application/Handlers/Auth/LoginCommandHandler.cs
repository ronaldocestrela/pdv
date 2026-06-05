using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Auth;
using Pdv.Application.Commands.Auth;

namespace Pdv.Application.Handlers.Auth;

public sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtService jwt) : IRequestHandler<LoginCommand, TokenResponseDto?>
{
    private readonly IUserRepository _users = users;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtService _jwt = jwt;

    /// <summary>
    /// Executes the <see cref="Login"/> to perform the corresponding business action.
    /// </summary>
    public async Task<TokenResponseDto?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetWithPermissionsByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null || !user.IsActive)
            return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var (accessToken, expiresAtUtc) = _jwt.CreateAccessToken(user);
        var refresh = _jwt.GenerateRefreshToken();
        var refreshExpiry = _jwt.GetRefreshTokenExpiresAtUtc();

        user.RefreshToken = refresh;
        user.RefreshTokenExpiresAtUtc = refreshExpiry;
        await _users.SaveChangesAsync(cancellationToken);

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static x => x, StringComparer.Ordinal)
            .ToList();

        return new TokenResponseDto(
            accessToken,
            refresh,
            expiresAtUtc,
            permissions,
            user.Id,
            user.Email,
            user.TenantId);
    }
}
