using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.Update;

internal sealed class UpdateOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .Include(o => o.Car)
            .Include(o => o.Insurance)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound("Order.NotFound", "Order not found"));
        }

        if (request.LeaseEndDate < request.LeaseStartDate ||
            request.LeaseStartDate < DateTime.UtcNow ||
            request.LeaseEndDate < DateTime.UtcNow)
        {
            return Result.Failure<Guid>(OrderErrors.InvalidLeaseTime);
        }

        int leaseDays = (request.LeaseEndDate - request.LeaseStartDate).Days;

        if (leaseDays == 0)
        {
            leaseDays = 1;
        }

        float totalPrice =
            leaseDays * order.Car.Price +
            leaseDays * order.Insurance.PricePerDay +
            order.Insurance.DeductibleAmount;

        order.LeaseStartDate = request.LeaseStartDate;
        order.LeaseEndDate = request.LeaseEndDate;
        order.TotalPrice = totalPrice;
        order.Currency = request.Currency;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(
                Error.Failure("Order.DatabaseError", ex.Message));
        }

        return order.Id;
    }
}
