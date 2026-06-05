using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Pdv.Modules.Stock.Application.Abstractions;
using Pdv.Modules.Stock.Infrastructure.Persistence;
using Pdv.Modules.Stock.Infrastructure.Persistence.Repositories;

namespace Pdv.Modules.Stock;

public static class DependencyInjection
{
    public static IServiceCollection AddStockModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IStockRepository, StockRepository>();

        services.AddDbContext<StockDbContext>(options =>
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
