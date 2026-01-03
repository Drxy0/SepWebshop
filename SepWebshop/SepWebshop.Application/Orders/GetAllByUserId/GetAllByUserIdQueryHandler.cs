using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.GetAllByUserId;

internal sealed class GetOrdersByUserIdQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetAllByUserIdQuery, Result<IReadOnlyList<Order>>>
{
    public async Task<Result<IReadOnlyList<Order>>> Handle(
        GetAllByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Order> orders = await context.Orders
            .Where(o => o.UserId == request.UserId)
            .Include(o => o.Car)
            .Include(o => o.User)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<Order>>(orders);
    }
}
