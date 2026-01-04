using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.GetById;

internal sealed class GetOrderByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        OrderDto? order = await context.Orders
            .Include(o => o.User)
            .Include(o => o.Car)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDto>(
                Error.NotFound("Order.NotFound", "Order not found"));
        }

        return order;
    }
}
