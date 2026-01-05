using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.Delete;

internal sealed class DeleteOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<Guid>(OrderErrors.NotFound(request.OrderId));
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
