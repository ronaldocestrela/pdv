using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pdv.Application.Behaviors;
using System.Reflection;

namespace Pdv.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
