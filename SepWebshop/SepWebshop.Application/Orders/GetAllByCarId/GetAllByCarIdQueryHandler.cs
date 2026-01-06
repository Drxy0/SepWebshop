using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Orders.DTOs;
using SepWebshop.Domain;

namespace SepWebshop.Application.Orders.GetAllByCarId;

internal class GetAllByCarIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetAllByCarIdQuery, Result<IReadOnlyList<OrderDto>>>
{
    public async Task<Result<IReadOnlyList<OrderDto>>> Handle(GetAllByCarIdQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<OrderDto> orders = await context.Orders
                    .Where(o => o.CarId == request.CarId)
                    .Select(o => new OrderDto
                    {
                        Id = o.Id,
                        UserId = o.UserId,
                        CarId = o.CarId,
                        InsuranceId = o.InsuranceId,
                        LeaseStartDate = o.LeaseStartDate,
                        LeaseEndDate = o.LeaseEndDate,
                        TotalPrice = o.TotalPrice,
                        Currency = o.Currency,
                        OrderStatus = o.OrderStatus,
                        PaymentMethod = o.PaymentMethod
                    })
                    .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<OrderDto>>(orders);

    }
}
