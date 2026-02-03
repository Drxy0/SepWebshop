using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CardService.Application.Abstractions.Bank;
using CardService.Domain.Contracts;
using CardService.Infrastructure.Bank;
using CardService.Infrastructure.Data;
using CardService.Infrastructure.Repository;
using CardService.Domain.Settings;

namespace CardService.Infrastructure
{
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
            services.Configure<PspSettings>(configuration.GetSection("PSP"));

            services.AddHttpClient("DataServiceClient", client =>
            {
                client.BaseAddress = new Uri(configuration["ApiSettings:DataServiceBaseUrl"] ?? throw new Exception("DataService URL is missing"));
            });

            services.AddHttpClient("WebShopClient", client =>
            {
                var baseUrl = configuration["ApiSettings:WebShopApiUrl"]
                    ?? throw new Exception("WebShopApiUrl is missing");
                client.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }
    }
}
