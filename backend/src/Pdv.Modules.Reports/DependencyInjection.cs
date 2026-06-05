using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Pdv.Modules.Reports.Application.Abstractions;
using Pdv.Modules.Reports.Infrastructure.Persistence;
using Pdv.Modules.Reports.Infrastructure.Repositories;

namespace Pdv.Modules.Reports;

public static class DependencyInjection
{
    public static IServiceCollection AddReportsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IReportRepository, ReportRepository>();

        services.AddDbContext<ReportsDbContext>(options =>
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
