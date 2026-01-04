using MediatR;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.Create;

internal sealed class CreateOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        OrderDto order = new()
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CarId = request.CarId,
            InsuranceId = request.InsuranceId,
            LeaseStartDate = request.LeaseStartDate,
            LeaseEndDate = request.LeaseEndDate,
            TotalPrice = request.TotalPrice,
            PaymentMethod = request.PaymentMethod
        };

        try
        {
            context.Orders.Add(order);
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
