using DataService.Contracts;
using DataService.Models;
using DataService.Persistance;
using DataService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DataService.Services;

public class PaymentService : IPaymentService
{
    private readonly DataServiceDbContext _dbContext;
    private readonly CarWebShopOptions _shopOptions;

    public PaymentService(DataServiceDbContext dbContext, IOptions<CarWebShopOptions> shopOptions)
    {
        _dbContext = dbContext;
        _shopOptions = shopOptions.Value;
    }

    public async Task<PaymentInitializationResult> InitializePaymentAsync(InitializePaymentRequest request)
    {
        if (request.MerchantId != _shopOptions.MerchantId ||
            request.MerchantPassword != _shopOptions.MerchantPassword)
        {
            return new PaymentInitializationResult(null, false);
        }

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
    public async Task<GetPaymentResponse?> GetPaymentByOrderIdAsync(string merchantOrderId)
    {
        return await _dbContext.Payments
            .Where(p => p.MerchantOrderId == merchantOrderId)
            .Select(p => new GetPaymentResponse
            {
                Id = p.Id,
                MerchantId = p.MerchantId,
                MerchantPassword = p.MerchantPassword,
                Amount = p.Amount,
                Currency = p.Currency,
                MerchantOrderId = p.MerchantOrderId,
                MerchantTimestamp = p.MerchantTimestamp
            })
            .FirstOrDefaultAsync();
    }

}
