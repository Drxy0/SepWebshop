using DataService.Contracts;
using DataService.Models;
using DataService.Persistance;
using DataService.Services.Interfaces;

namespace DataService.Services;

public class PaymentService : IPaymentService
{
    private readonly DataServiceDbContext _dbContext;

    public PaymentService(DataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentInitializationResult> InitializePaymentAsync(InitializePaymentRequest request)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            MerchantPassword = request.MerchantPassword,
            Amount = request.Amount,
            Currency = request.currency,
            MerchantOrderId = request.MerchantOrderId,
            MerchantTimestamp = request.MerchantTimestamp
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        return new PaymentInitializationResult(payment.Id.ToString(), true);
    }
}
