using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.GetAllByCarId;

internal class GetAllByCarIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllByCarIdQuery, Result<IReadOnlyList<Order>>>
{
    public async Task<Result<IReadOnlyList<Order>>> Handle(GetAllByCarIdQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Order> orders = await context.Orders
            .Where(o => o.CarId == request.CarId)
            .Include(o => o.Car)
            .Include(o => o.User)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<Order>>(orders);
    }
}
