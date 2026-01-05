using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace SepWebshop.Application.Orders.Services;

public class CancelExpiredOrdersService
{
    private readonly IApplicationDbContext _context;

    public CancelExpiredOrdersService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CancelUnpaidOrdersAsync(CancellationToken cancellationToken)
    {
        DateTime cutoff = DateTime.UtcNow.AddMinutes(-15);

        List<Order> orders = await _context.Orders
            .Where(o => o.OrderStatus == OrderStatus.PendingPayment && o.CreatedAt <= cutoff)
            .ToListAsync(cancellationToken);

        foreach (Order order in orders)
        {
            order.OrderStatus = OrderStatus.Cancelled;
        }

        if (orders.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);
    }
}
