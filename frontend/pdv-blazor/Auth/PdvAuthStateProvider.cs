using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Pdv.Web.Models;

namespace Pdv.Web.Auth;

/// <summary>
/// Equivalente ao useAuthStore (Zustand) do React.
/// Persiste a sessão em localStorage via Blazored.LocalStorage.
/// Expõe: AccessToken, Permissions e estado de autenticação ao sistema de Authorization do Blazor.
/// </summary>
public class PdvAuthStateProvider(ILocalStorageService localStorage) : AuthenticationStateProvider
{
    private const string SessionKey = "pdv-auth";
    private AuthSession? _session;

    // ─── AuthenticationStateProvider ────────────────────────────────────────

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _session ??= await LoadSessionAsync();

        if (string.IsNullOrEmpty(_session?.AccessToken))
            return Unauthenticated();

        var claims = BuildClaims(_session);
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    // ─── Acesso público ──────────────────────────────────────────────────────

    public async Task<string?> GetAccessTokenAsync()
    {
        _session ??= await LoadSessionAsync();
        return _session?.AccessToken;
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        _session ??= await LoadSessionAsync();
        return _session?.RefreshToken;
    }

    public bool Can(string permission)
        => _session?.Permissions.Contains(permission) == true;

    // ─── Mutações (equivalente a setSession / logout do Zustand) ────────────

    public async Task SetSessionAsync(AuthTokenResponse response)
    {
        _session = new AuthSession
        {
            AccessToken  = response.AccessToken,
            RefreshToken = response.RefreshToken,
            UserId       = response.UserId,
            Email        = response.Email,
            TenantId     = response.TenantId,
            Permissions  = response.Permissions,
            ExpiresAtUtc = response.ExpiresAtUtc,
        };

        await localStorage.SetItemAsync(SessionKey, _session);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        _session = null;
        await localStorage.RemoveItemAsync(SessionKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Unauthenticated()));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<AuthSession?> LoadSessionAsync()
    {
        try { return await localStorage.GetItemAsync<AuthSession>(SessionKey); }
        catch { return null; }
    }

    private static AuthenticationState Unauthenticated()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static IEnumerable<Claim> BuildClaims(AuthSession s)
    {
        var claims = new List<Claim>();
        if (!string.IsNullOrEmpty(s.UserId))    claims.Add(new Claim(ClaimTypes.NameIdentifier, s.UserId));
        if (!string.IsNullOrEmpty(s.Email))     claims.Add(new Claim(ClaimTypes.Email, s.Email));
        if (!string.IsNullOrEmpty(s.TenantId))  claims.Add(new Claim("tenant_id", s.TenantId));
        foreach (var p in s.Permissions)        claims.Add(new Claim("permission", p));
        return claims;
    }
}
