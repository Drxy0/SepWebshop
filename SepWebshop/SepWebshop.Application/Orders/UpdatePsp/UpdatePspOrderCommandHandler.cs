using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.UpdatePsp;
public class UpdatePspOrderCommandHandler(IApplicationDbContext context,
    IOptions<PspSettings> pspOptions) : IRequestHandler<UpdatePspOrderCommand, Result<Guid>>
{
    private readonly PspSettings _pspSettings = pspOptions.Value;
    public async Task<Result<Guid>> Handle(UpdatePspOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.PspId != _pspSettings.PspId || request.PspPassword != _pspSettings.PspPassword)
        {
            return Result.Failure<Guid>(
                Error.Failure("Auth.InvalidCredentials", "Wrong PSP ID or password."));
        }

        Order? order = await context.Orders
            .Include(o => o.Car)
            .Include(o => o.Insurance)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound("Order.NotFound", "Order not found"));
        }

        order.OrderStatus = request.OrderStatus;
        order.PaymentMethod = request.PaymentMethod;

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
