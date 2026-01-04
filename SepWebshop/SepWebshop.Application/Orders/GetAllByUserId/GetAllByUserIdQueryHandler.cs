using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Application.Orders.DTOs;
using SepWebshop.Domain;

namespace SepWebshop.Application.Orders.GetAllByUserId;

internal sealed class GetOrdersByUserIdQueryHandler
    : IRequestHandler<GetAllByUserIdQuery, Result<IReadOnlyList<OrderDto>>>
{
    private readonly IApplicationDbContext context;

    public GetOrdersByUserIdQueryHandler(IApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> Handle(
        GetAllByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var orderDtos = await context.Orders
            .Where(o => o.UserId == request.UserId)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                CarId = o.CarId,
                InsuranceId = o.InsuranceId,
                LeaseStartDate = o.LeaseStartDate,
                LeaseEndDate = o.LeaseEndDate,
                TotalPrice = o.TotalPrice,
                IsCompleted = o.IsCompleted,
                PaymentMethod = o.PaymentMethod
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<OrderDto>>(orderDtos);
    }
}
