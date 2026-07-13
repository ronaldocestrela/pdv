namespace Pdv.Web.Models;

/// <summary>Resposta de POST /api/auth/login e /api/auth/refresh — espelho de AuthTokenResponse do React.</summary>
public class AuthTokenResponse
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string ExpiresAtUtc { get; set; } = "";
    public List<string> Permissions { get; set; } = [];
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string? TenantId { get; set; }
}

/// <summary>Sessão armazenada no localStorage (equivalente ao estado do Zustand).</summary>
public class AuthSession
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? TenantId { get; set; }
    public List<string> Permissions { get; set; } = [];
    public string? ExpiresAtUtc { get; set; }
}
