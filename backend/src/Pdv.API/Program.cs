using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Pdv.API.Middleware;
using Pdv.Shared.Kernel.Security;
using Pdv.Modules.Identity;
using Pdv.Modules.Identity.Infrastructure.Persistence;
using Pdv.Modules.Identity.Infrastructure.Persistence.Seed;
using Pdv.Modules.Identity.Infrastructure.Services;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Catalog;
using Pdv.Modules.Catalog.Infrastructure.Persistence;
using Pdv.Modules.Stock;
using Pdv.Modules.Stock.Infrastructure.Persistence;
using Pdv.Modules.Sales;
using Pdv.Modules.Sales.Infrastructure.Persistence;
using Pdv.Modules.Reports;
using Pdv.Modules.Reports.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                      ?? ["http://localhost:1234"];
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure Shared Kernel and Infrastructure Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Pdv.Shared.Kernel.Abstractions.ITenantContext, Pdv.Shared.Kernel.Services.HttpTenantContext>();
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Pdv.Shared.Kernel.Behaviors.ValidationBehavior<,>));

// Configure Modular Monolith Modules
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddStockModule(builder.Configuration);
builder.Services.AddSalesModule(builder.Configuration);
builder.Services.AddReportsModule(builder.Configuration);

var jwtOpts = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
              ?? throw new InvalidOperationException("Jwt configuration section missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOpts.Issuer,
        ValidAudience = jwtOpts.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.Key)),
        ClockSkew = TimeSpan.FromMinutes(2),
    };
});

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in KnownPermissions.All)
        options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));

    options.AddPolicy(Pdv.Modules.Identity.Controllers.PermissionsController.AdminRolesReadPolicy, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("permission", KnownPermissions.RoleManage) ||
            ctx.User.HasClaim("permission", KnownPermissions.UserManage)));

    options.AddPolicy(KnownPermissions.TenantManage, policy =>
        policy.RequireClaim("is_super_admin", "true")
              .RequireClaim("permission", KnownPermissions.TenantManage));
});

builder.Services.AddControllers()
    .AddApplicationPart(typeof(Pdv.Modules.Identity.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Pdv.Modules.Catalog.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Pdv.Modules.Stock.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Pdv.Modules.Sales.DependencyInjection).Assembly)
    .AddApplicationPart(typeof(Pdv.Modules.Reports.DependencyInjection).Assembly)
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var sp = scope.ServiceProvider;
    var useInMemory = app.Configuration.GetValue("Database:UseInMemory", false);

    var identityDb = sp.GetRequiredService<IdentityDbContext>();
    var catalogDb = sp.GetRequiredService<CatalogDbContext>();
    var salesDb = sp.GetRequiredService<SalesDbContext>();
    var stockDb = sp.GetRequiredService<StockDbContext>();
    var reportsDb = sp.GetRequiredService<ReportsDbContext>();

    if (useInMemory)
    {
        await identityDb.Database.EnsureCreatedAsync();
        await catalogDb.Database.EnsureCreatedAsync();
        await salesDb.Database.EnsureCreatedAsync();
        await stockDb.Database.EnsureCreatedAsync();
        await reportsDb.Database.EnsureCreatedAsync();
    }
    else
    {
        await identityDb.Database.MigrateAsync();
        await catalogDb.Database.MigrateAsync();
        await salesDb.Database.MigrateAsync();
        await stockDb.Database.MigrateAsync();
    }

    var seed = sp.GetRequiredService<IOptions<SeedOptions>>().Value;
    var pwd = sp.GetRequiredService<IPasswordHasher>();
    var lf = sp.GetRequiredService<ILoggerFactory>();
    await DbSeeder.ApplyAsync(identityDb, seed, pwd, lf.CreateLogger("DbSeeder"), CancellationToken.None);
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ApiExceptionHandlingMiddleware>();
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
