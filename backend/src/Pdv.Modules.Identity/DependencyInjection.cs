using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentValidation;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Infrastructure.Persistence;
using Pdv.Modules.Identity.Infrastructure.Persistence.Repositories;
using Pdv.Modules.Identity.Infrastructure.Persistence.Seed;
using Pdv.Modules.Identity.Infrastructure.Services;

namespace Pdv.Modules.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        services.AddSingleton<IOptions<JwtOptions>>(_ =>
        {
            var opts = new JwtOptions();
            configuration.GetSection(JwtOptions.SectionName).Bind(opts);
            if (string.IsNullOrWhiteSpace(opts.Key))
                throw new InvalidOperationException("Jwt:Key is required.");
            return Options.Create(opts);
        });

        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserAdminRepository, UserAdminRepository>();

        services.AddDbContext<IdentityDbContext>(options =>
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

        // Registra os handlers de MediatR e os validadores de FluentValidation deste assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
