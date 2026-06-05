namespace Pdv.Application.Auth;

public sealed record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    IReadOnlyList<string> Permissions,
    int UserId,
    string Email,
    int TenantId);
