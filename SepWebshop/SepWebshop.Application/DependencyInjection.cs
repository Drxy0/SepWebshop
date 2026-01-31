using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SepWebshop.Application.Orders.UpdatePsp;

namespace SepWebshop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssemblyContaining(typeof(DependencyInjection));
        });

        services.Configure<PspSettings>(configuration.GetSection(PspSettings.SectionName));

        return services;
    }
}

public class PspSettings
{
    public const string SectionName = "PSP";
    public string PspId { get; set; } = string.Empty;
    public string PspPassword { get; set; } = string.Empty;
}
