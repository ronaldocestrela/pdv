namespace Pdv.Modules.Identity.Application.Auth;

public sealed record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    IReadOnlyList<string> Permissions,
    Guid UserId,
    string Email,
    Guid TenantId);
