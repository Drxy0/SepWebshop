using MediatR;
using Microsoft.Extensions.Options;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Abstractions.Payment;
using SepWebshop.Domain;
using SepWebshop.Domain.Cars;
using SepWebshop.Domain.Insurances;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.Create;

internal sealed class CreateOrderCommandHandler(
    IApplicationDbContext context,
    IPaymentService paymentService,
    IOptions<PspOptions> pspOptions) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly PspOptions _pspOptions = pspOptions.Value;

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {

        Car? car = await context.Cars.FindAsync(request.CarId, cancellationToken);

        if (car is null)
        {
            return Result.Failure<Guid>(CarErrors.NotFound(request.CarId));
        }

        Insurance? insurance = await context.Insurances.FindAsync(request.InsuranceId, cancellationToken);

        if (insurance is null)
        {
            return Result.Failure<Guid>(InsuranceErrors.NotFound(request.InsuranceId));
        }

        if (request.LeaseEndDate < request.LeaseStartDate || 
            request.LeaseStartDate < DateTime.UtcNow.AddDays(-1) || 
            request.LeaseEndDate < DateTime.UtcNow)
        {
            return Result.Failure<Guid>(OrderErrors.InvalidLeaseTime);
        }

        int leaseDays = (request.LeaseEndDate - request.LeaseStartDate).Days;
        
        if (leaseDays == 0)
        {
            leaseDays = 1;
        }

        float totalPrice = 
            leaseDays * car.Price + 
            leaseDays * insurance.PricePerDay + 
            insurance.DeductibleAmount;

        Order order = new()
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CarId = request.CarId,
            InsuranceId = request.InsuranceId,
            LeaseStartDate = request.LeaseStartDate,
            LeaseEndDate = request.LeaseEndDate,
            TotalPrice = totalPrice,
            Currency = request.Currency
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

        // Send order data to PSP
        string merchantOrderId = order.Id.ToString("N");
        DateTime merchantTimestamp = DateTime.UtcNow;

        var paymentInitResult = await paymentService.InitializePaymentAsync(
            merchantId: _pspOptions.MerchantId,
            merchantPassword: _pspOptions.MerchantPassword,
            amount: (double)totalPrice,
            currency: order.Currency,
            merchantOrderId: merchantOrderId,
            merchantTimestamp: merchantTimestamp,
            cancellationToken: cancellationToken);

        if (!paymentInitResult.IsSuccess)
        {
            return Result.Failure<Guid>(
                Error.Failure("Order.PaymentInitializationFailed", "Failed to initialize payment with PSP"));
        }

        return order.Id;
    }
}
