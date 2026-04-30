using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pdv.Application.Abstractions;
using Pdv.Infrastructure.Persistence;
using Pdv.Infrastructure.Repositories;
using Pdv.Infrastructure.Seed;
using Pdv.Infrastructure.Services;

namespace Pdv.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IOptions<JwtOptions>>(_ =>
        {
            var opts = new JwtOptions();
            configuration.GetSection(JwtOptions.SectionName).Bind(opts);
            if (string.IsNullOrWhiteSpace(opts.Key))
                throw new InvalidOperationException("Jwt:Key is required.");
            return Options.Create(opts);
        });

        services.AddSingleton<IOptions<SeedOptions>>(_ =>
        {
            var opts = new SeedOptions();
            configuration.GetSection(SeedOptions.SectionName).Bind(opts);
            return Options.Create(opts);
        });

        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserAdminRepository, UserAdminRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        services.AddDbContext<AppDbContext>(options =>
        {
            if (configuration.GetValue("Database:UseInMemory", false))
            {
                var inMemoryName = configuration["Database:InMemoryDatabaseName"];
                options.UseInMemoryDatabase(
                    string.IsNullOrWhiteSpace(inMemoryName) ? "PdvInMemory" : inMemoryName);
            }
            else
            {
                var cs = configuration.GetConnectionString("DefaultConnection")
                         ?? throw new InvalidOperationException(
                             "ConnectionStrings:DefaultConnection is required when Database:UseInMemory is false.");
                options.UseSqlServer(cs);
            }
        });

        return services;
    }
}
