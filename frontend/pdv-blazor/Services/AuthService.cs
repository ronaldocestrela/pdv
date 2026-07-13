using System.Net.Http.Json;
using Pdv.Web.Auth;
using Pdv.Web.Models;

namespace Pdv.Web.Services;

/// <summary>Equivalente a services/auth.ts — login e refresh via API.</summary>
public class AuthService(HttpClient http, PdvAuthStateProvider authProvider)
{
    /// <summary>POST /api/auth/login — equivalente à função login() do React.</summary>
    public async Task<AuthTokenResponse> LoginAsync(string email, string password)
    {
        var response = await http.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<AuthTokenResponse>()
                   ?? throw new Exception("Resposta inválida do servidor.");
        await authProvider.SetSessionAsync(data);
        return data;
    }

    public Task LogoutAsync() => authProvider.LogoutAsync();
}
