using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SepWebshop.Application.Orders.Services;

namespace SepWebshop.Infrastructure.BackgroundTasks;
public class ExpiredOrdersBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredOrdersBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
        logger.LogInformation("Expired Orders Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var cancelService = scope.ServiceProvider.GetRequiredService<CancelExpiredOrdersService>();

                logger.LogInformation("Running task: Canceling expired unpaid orders...");

                await cancelService.CancelUnpaidOrdersAsync(stoppingToken);

                logger.LogInformation("Successfully processed expired orders.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing expired orders.");
            }

            await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
        }
    }
}