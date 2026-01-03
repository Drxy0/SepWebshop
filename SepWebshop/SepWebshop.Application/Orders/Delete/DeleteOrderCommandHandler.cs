using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;

namespace SepWebshop.Application.Orders.Delete;

internal sealed class DeleteOrderCommandHandler(
    IApplicationDbContext context
) : IRequestHandler<DeleteOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        DeleteOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound("Order.NotFound", "Order not found"));
        }

        context.Orders.Remove(order);

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
