using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pdv.Web.Auth;
using Pdv.Web.Models;
using Pdv.Web.Services;

namespace Pdv.Web.Tests;

/// <summary>
/// Classe base para testes de componentes Blazor com bUnit.
/// Configura o contêiner de DI com mocks das dependências e serviços reais com HttpClient simulado.
/// </summary>
public class BlazorTestBase : TestContext
{
    protected Mock<ILocalStorageService> MockLocalStorage { get; } = new(MockBehavior.Strict);
    protected FakeHttpMessageHandler HttpHandler { get; } = new();
    protected PdvAuthStateProvider AuthProvider { get; }

    public BlazorTestBase()
    {
        // 1. Configurar bUnit Fake Navigation Manager
        this.JSInterop.Mode = JSRuntimeMode.Loose;

        // 2. LocalStorage mock padrão
        MockLocalStorage.Setup(x => x.GetItemAsync<AuthSession>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthSession?)null);
        this.Services.AddSingleton<ILocalStorageService>(MockLocalStorage.Object);

        // 3. Provedor de estado de autenticação real
        AuthProvider = new PdvAuthStateProvider(MockLocalStorage.Object);
        this.Services.AddSingleton<PdvAuthStateProvider>(AuthProvider);
        this.Services.AddSingleton<AuthenticationStateProvider>(AuthProvider);
        this.Services.AddAuthorizationCore();

        // 4. Configurar HttpClient simulado
        var httpClient = new HttpClient(HttpHandler) { BaseAddress = new Uri("http://localhost") };
        this.Services.AddSingleton<HttpClient>(httpClient);

        // 5. Registrar os serviços de API reais (para testar a integração HttpClient)
        this.Services.AddSingleton<AuthService>();
        this.Services.AddSingleton<ProductsService>();
        this.Services.AddSingleton<SalesService>();
        this.Services.AddSingleton<StockService>();
        this.Services.AddSingleton<SuppliersService>();
        this.Services.AddSingleton<ReportsService>();
        this.Services.AddSingleton<RolesService>();
        this.Services.AddSingleton<UsersService>();
        this.Services.AddSingleton<TenantsService>();
        this.Services.AddSingleton<PermissionsService>();
    }
}

/// <summary>
/// Simulador simples de requisições HTTP por rota e método.
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>> _routes = new();

    public void Setup(string method, string path, HttpStatusCode statusCode, object? responseBody = null, int delayMs = 0)
    {
        // Limpar queries da rota para correspondência simples
        var key = $"{method.ToUpper()}:{path.Split('?')[0]}";
        _routes[key] = async (req) =>
        {
            if (delayMs > 0)
            {
                await Task.Delay(delayMs);
            }

            var content = responseBody != null
                ? new StringContent(JsonSerializer.Serialize(responseBody), Encoding.UTF8, "application/json")
                : null;

            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content
            };
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? "";
        var key = $"{request.Method.Method.ToUpper()}:{path}";

        if (_routes.TryGetValue(key, out var handler))
        {
            return await handler(request);
        }

        // Retorna 404 por padrão para rotas não configuradas
        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }
}
