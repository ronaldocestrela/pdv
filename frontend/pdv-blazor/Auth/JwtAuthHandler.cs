using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Pdv.Web.Models;

namespace Pdv.Web.Auth;

/// <summary>
/// Equivalente aos interceptors do Axios do React.
/// Injeta o Bearer token em cada requisição.
/// Em caso de 401, tenta refresh automático; se falhar, redireciona para /login.
/// </summary>
public class JwtAuthHandler(
    PdvAuthStateProvider authProvider,
    NavigationManager    nav) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Não injeta token em rotas de auth
        var isAuthRoute = request.RequestUri?.PathAndQuery.StartsWith("/api/auth/") == true
                       || request.RequestUri?.PathAndQuery.StartsWith("/api/tenants/register") == true;

        if (!isAuthRoute)
        {
            var token = await authProvider.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Intercept 401 — tenta refresh (equivalente ao interceptor do Axios)
        if (response.StatusCode == HttpStatusCode.Unauthorized && !isAuthRoute)
        {
            var refreshToken = await authProvider.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshed = await TryRefreshAsync(request.RequestUri!.GetLeftPart(UriPartial.Authority),
                                                      refreshToken, cancellationToken);
                if (refreshed is not null)
                {
                    await authProvider.SetSessionAsync(refreshed);

                    // Rerepete a requisição original com o novo token
                    var clone = await CloneRequestAsync(request);
                    clone.Headers.Authorization = new("Bearer", refreshed.AccessToken);
                    return await base.SendAsync(clone, cancellationToken);
                }
            }

            // Refresh falhou → logout + redireciona
            await authProvider.LogoutAsync();
            nav.NavigateTo("/login", replace: true);
        }

        return response;
    }

    private async Task<AuthTokenResponse?> TryRefreshAsync(
        string baseUrl, string refreshToken, CancellationToken ct)
    {
        try
        {
            using var client = new HttpClient();
            var res = await client.PostAsJsonAsync(
                $"{baseUrl}/api/auth/refresh",
                new { refreshToken },
                ct);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<AuthTokenResponse>(ct);
        }
        catch { return null; }
    }

    /// <summary>Clona a requisição original para reenvio com novo token.</summary>
    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri);
        foreach (var h in req.Headers) clone.Headers.TryAddWithoutValidation(h.Key, h.Value);

        if (req.Content is not null)
        {
            var bytes = await req.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
            foreach (var h in req.Content.Headers) clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }
        return clone;
    }
}
