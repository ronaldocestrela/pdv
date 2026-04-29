using Pdv.Domain.Entities;

namespace Pdv.Application.Abstractions;

/// <summary>JWT access token + opaque refresh token generation.</summary>
public interface IJwtService
{
    (string AccessToken, DateTime ExpiresAtUtc) CreateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiresAtUtc();
}
