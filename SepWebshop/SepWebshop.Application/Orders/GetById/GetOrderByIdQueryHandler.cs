using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Orders.DTOs;
using SepWebshop.Domain;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.GetById;

internal sealed class GetOrderByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .Include(o => o.User)
            .Include(o => o.Car)
            .Include(o => o.Insurance)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDto>(OrderErrors.NotFound(request.OrderId));
        }

        var orderDto = new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            CarId = order.CarId,
            InsuranceId = order.InsuranceId,
            LeaseStartDate = order.LeaseStartDate,
            LeaseEndDate = order.LeaseEndDate,
            TotalPrice = order.TotalPrice,
            OrderStatus = order.OrderStatus,
            PaymentMethod = order.PaymentMethod
        };

        return Result.Success(orderDto);
    }
}
