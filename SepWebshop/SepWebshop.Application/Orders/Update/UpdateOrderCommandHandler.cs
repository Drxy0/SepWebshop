using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;

namespace SepWebshop.Application.Orders.Update;

internal sealed class UpdateOrderCommandHandler(
    IApplicationDbContext context
) : IRequestHandler<UpdateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound("Order.NotFound", "Order not found"));
        }

        order.LeaseStartDate = request.LeaseStartDate;
        order.LeaseEndDate = request.LeaseEndDate;
        order.TotalPrice = request.TotalPrice;
        order.PaymentMethod = request.PaymentMethod;

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
