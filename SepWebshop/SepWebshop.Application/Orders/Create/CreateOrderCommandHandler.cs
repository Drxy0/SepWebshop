using MediatR;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Cars;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.Create;

internal sealed class CreateOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {

        Car? car = await context.Cars.FindAsync(request.CarId, cancellationToken);

        if (car is null)
        {
            return Result.Failure<Guid>(CarErrors.NotFound(request.CarId));
        }

        if (request.LeaseEndDate < request.LeaseStartDate || 
            request.LeaseStartDate < DateTime.UtcNow || 
            request.LeaseEndDate < DateTime.UtcNow)
        {
            return Result.Failure<Guid>(OrderErrors.InvalidLeaseTime);
        }

        int leaseDays = (request.LeaseEndDate - request.LeaseStartDate).Days;
        
        if (leaseDays == 0)
        {
            leaseDays = 1;
        }

        Order order = new()
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CarId = request.CarId,
            InsuranceId = request.InsuranceId,
            LeaseStartDate = request.LeaseStartDate,
            LeaseEndDate = request.LeaseEndDate,
            TotalPrice = leaseDays * car.Price
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
