using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pdv.Application.Behaviors;
using System.Reflection;

namespace Pdv.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Creates a new instance in the system.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
