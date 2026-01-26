using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using QrService.Application.Abstractions.Bank;
using QrService.Application.Common.Behaviours;
using System.Reflection;

namespace QrService.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        return services;
    }
}
