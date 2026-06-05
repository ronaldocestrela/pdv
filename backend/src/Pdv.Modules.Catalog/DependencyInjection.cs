using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Infrastructure.Persistence;
using Pdv.Modules.Catalog.Infrastructure.Persistence.Repositories;

namespace Pdv.Modules.Catalog;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICatalogRepository, CatalogRepository>();

        services.AddDbContext<CatalogDbContext>(options =>
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
