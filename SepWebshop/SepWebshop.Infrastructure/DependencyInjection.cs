using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SepWebshop.Application.Abstractions.Authentication;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Abstractions.Email;
using SepWebshop.Application.Abstractions.Payment;
using SepWebshop.Application.Orders.Services;
using SepWebshop.Infrastructure.Authentication;
using SepWebshop.Infrastructure.BackgroundTasks;
using SepWebshop.Infrastructure.Email;
using SepWebshop.Infrastructure.Payment;
using System.Security.Claims;
using System.Text;

namespace SepWebshop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(connectionString));

        services.AddDbContext<ApplicationDbContext>();

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtGenerator, JwtGenerator>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)
                    ),

                    RoleClaimType = ClaimTypes.Role
                };
            });

        // Email
        services.Configure<SendGridOptions>(configuration.GetSection(SendGridOptions.SectionName));
        services.AddScoped<IEmailSender, SendGridEmailSender>();

        // Payment Service - Configure HttpClient
        services.AddHttpClient<IPaymentService, PaymentService>(client =>
        {
            var baseUrl = configuration["Payment:BaseUrl"]
                ?? throw new InvalidOperationException("Payment:BaseUrl not configured");
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });


        // Background service for auto-cancelling orders
        services.AddScoped<CancelExpiredOrdersService>();
        services.AddHostedService<ExpiredOrdersBackgroundService>();

        return services;
    }
}
