using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.GetById;

internal sealed class GetOrderByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetOrderByIdQuery, Result<Order>>
{
    public async Task<Result<Order>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .Include(o => o.User)
            .Include(o => o.Car)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<Order>(
                Error.NotFound("Order.NotFound", "Order not found"));
        }

        return order;
    }
}
