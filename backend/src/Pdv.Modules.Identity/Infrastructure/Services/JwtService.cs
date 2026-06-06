using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Domain.Entities;
using Pdv.Shared.Kernel.Security;

namespace Pdv.Modules.Identity.Infrastructure.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
}

/// <summary>
/// Initializes a new instance of the <see cref="JwtService"/> class.
/// </summary>
public sealed class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    private readonly JwtOptions _options = options.Value;

    public (string AccessToken, DateTime ExpiresAtUtc) CreateAccessToken(User user)
    {
        var issuedAt = DateTimeOffset.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var permissions = FlattenPermissions(user);
        var isSuperAdmin = user.TenantId == KnownTenants.HostTenantId && user.UserRoles.Any(ur =>
            string.Equals(ur.Role.Name, KnownRoles.SuperAdmin, StringComparison.Ordinal));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim("is_super_admin", isSuperAdmin.ToString().ToLowerInvariant()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var p in permissions)
            claims.Add(new Claim("permission", p));

        var expiresAt = issuedAt.AddMinutes(_options.AccessTokenMinutes).UtcDateTime;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = credentials,
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return (handler.WriteToken(token), expiresAt);
    }

    /// <summary>
    /// Executes the GenerateRefreshToken operation.
    /// </summary>
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Retrieves data from the system.
    /// </summary>
    public DateTime GetRefreshTokenExpiresAtUtc()
    {
        return DateTime.UtcNow.AddDays(_options.RefreshTokenDays);
    }

    private static IReadOnlyList<string> FlattenPermissions(User user)
    {
        return user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static n => n, StringComparer.Ordinal)
            .ToList();
    }
}
