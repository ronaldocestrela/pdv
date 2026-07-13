using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pdv.Web.Auth;
using Pdv.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Pdv.Web.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── Configuração ────────────────────────────────────────────────────────────
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5190";

// ── LocalStorage (Blazored) ─────────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();

// ── Auth ─────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<PdvAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<PdvAuthStateProvider>());
builder.Services.AddAuthorizationCore();

// ── HttpClient com JWT handler ───────────────────────────────────────────────
// HttpClient "autenticado" — usado por todos os serviços de negócio
builder.Services.AddTransient<JwtAuthHandler>();
builder.Services.AddHttpClient("api", c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

// ── Serviços de negócio ──────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductsService>();
builder.Services.AddScoped<SalesService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<SuppliersService>();
builder.Services.AddScoped<ReportsService>();
builder.Services.AddScoped<RolesService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<TenantsService>();
builder.Services.AddScoped<PermissionsService>();


await builder.Build().RunAsync();
