using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Pdv.Maui.Services;
using Pdv.Web.Auth;
using Pdv.Web.Services;
using System;
using System.Net.Http;

namespace Pdv.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // ── Blazor WebView ──
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // ── Configuração de URL da API ──
        var apiBaseUrl = "http://localhost:5190";
#if ANDROID
        apiBaseUrl = "http://10.0.2.2:5190"; // Redirecionamento do emulador
#endif

        // ── LocalStorage (Blazored) ──
        builder.Services.AddBlazoredLocalStorage();

        // ── Banco SQLite Local ──
        builder.Services.AddSingleton<LocalDbService>();
        builder.Services.AddSingleton<ConnectionStatusService, MauiConnectionStatusService>();
        builder.Services.AddSingleton<SyncWorker>();

        // ── Auth ──
        builder.Services.AddScoped<PdvAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<PdvAuthStateProvider>());
        builder.Services.AddAuthorizationCore();

        // ── HttpClient com JWT Handler ──
        builder.Services.AddTransient<JwtAuthHandler>();
        builder.Services.AddHttpClient("api", c => c.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<JwtAuthHandler>();

        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

        // ── Serviços de Negócio (Substituídos por versões offline/sincronizadas) ──
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<ProductsService, MauiProductsService>();
        builder.Services.AddScoped<SalesService, MauiSalesService>();
        builder.Services.AddScoped<StockService, MauiStockService>();
        builder.Services.AddScoped<SuppliersService, MauiSuppliersService>();
        builder.Services.AddScoped<ReportsService>();
        builder.Services.AddScoped<RolesService>();
        builder.Services.AddScoped<UsersService>();
        builder.Services.AddScoped<TenantsService>();
        builder.Services.AddScoped<PermissionsService>();

        return builder.Build();
    }
}
