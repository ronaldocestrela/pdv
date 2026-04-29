using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Auth;
using Pdv.Application.Commands.Auth;

namespace Pdv.Application.Handlers.Auth;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponseDto?>
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;

    public RefreshTokenCommandHandler(IUserRepository users, IJwtService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<TokenResponseDto?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return null;

        var user = await _users.GetWithPermissionsByRefreshTokenAsync(request.RefreshToken.Trim(), cancellationToken);
        if (user is null || !user.IsActive)
            return null;

        if (user.RefreshTokenExpiresAtUtc is null || user.RefreshTokenExpiresAtUtc <= DateTime.UtcNow)
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
            user.Email);
    }
}
