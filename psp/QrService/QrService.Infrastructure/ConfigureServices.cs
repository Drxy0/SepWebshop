using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QrService.Application.Abstractions.Bank;
using QrService.Domain.Contracts;
using QrService.Infrastructure.Bank;
using QrService.Infrastructure.Data;
using QrService.Infrastructure.Repository;

namespace QrService.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddTransient<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IBankClient, BankClient>();

        services.AddHttpClient("DataServiceClient", client =>
        {
            client.BaseAddress = new Uri(configuration["ApiSettings:DataServiceBaseUrl"] ?? throw new Exception("DataService URL is missing"));
        });

        return services;
    }
}
