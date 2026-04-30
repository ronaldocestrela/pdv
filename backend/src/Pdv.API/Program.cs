using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Pdv.API.Controllers;
using Pdv.API.Middleware;
using Pdv.Application;
using Pdv.Application.Abstractions;
using Pdv.Infrastructure;
using Pdv.Infrastructure.Persistence;
using Pdv.Infrastructure.Seed;
using Pdv.Infrastructure.Services;
using Pdv.Application.Security;

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

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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

    options.AddPolicy(PermissionsController.AdminRolesReadPolicy, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim("permission", KnownPermissions.RoleManage) ||
            ctx.User.HasClaim("permission", KnownPermissions.UserManage)));
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Configuration.GetValue("Database:UseInMemory", false))
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    var seed = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;
    var pwd = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var lf = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    await DbSeeder.ApplyAsync(db, seed, pwd, lf.CreateLogger(nameof(DbSeeder)), CancellationToken.None);
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
