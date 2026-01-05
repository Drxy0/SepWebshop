using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SepWebshop.Application.Orders.Services;

namespace SepWebshop.Infrastructure.BackgroundTasks;

public class ExpiredOrdersBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            CancelExpiredOrdersService cancelService = scope.ServiceProvider.GetRequiredService<CancelExpiredOrdersService>();

            await cancelService.CancelUnpaidOrdersAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
