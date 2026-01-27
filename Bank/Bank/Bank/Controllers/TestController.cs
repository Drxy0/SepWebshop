using Bank.Contracts.QR;
using Bank.Models;
using Bank.Persistance;
using Bank.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bank.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly BankDbContext _context;

    public TestController(BankDbContext context)
    {
        _context = context;
    }

    [HttpPost("create-test-customer")]
    public async Task<IActionResult> CreateTestCustomer()
    {
        var customerAccount = new Account
        {
            AccountNumber = "123-456789-78",
            AccountHolderName = "Marko Markovic",
            Balance = 50000,
            Type = AccountType.Customer
        };

        var debitCard = new DebitCard
        {
            CardHolderName = "Marko Markovic",
            CardNumber = "4111111111111111",
            CVV = "123",
            ExpirationDate = "12/26",
            Account = customerAccount
        };

        _context.Accounts.Add(customerAccount);
        _context.DebitCards.Add(debitCard);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            customerAccount.Id,
            customerAccount.AccountNumber,
            customerAccount.AccountHolderName,
            customerAccount.Balance,
            CardNumber = debitCard.CardNumber
        });
    }

    [HttpPost("create-test-merchant")]
    public async Task<IActionResult> CreateTestMerchant()
    {
        var merchantAccount = new Account
        {
            AccountNumber = "555-123456-99",
            AccountHolderName = "Test Shop d.o.o.",
            Balance = 0,
            Type = AccountType.Merchant
        };

        var merchant = new Merchant
        {
            Id = "MERCHANT_TEST",
            Name = "Test Shop",
            AccountId = merchantAccount.Id,
            Account = merchantAccount
        };

        _context.Accounts.Add(merchantAccount);
        _context.Merchants.Add(merchant);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            merchant.Id,
            merchant.Name,
            AccountNumber = merchantAccount.AccountNumber,
            Balance = merchantAccount.Balance
        });
    }


    [HttpPost("simulate-ips-payment")]
    public async Task<IActionResult> SimulateIpsPayment(
        [FromBody] SimulateIpsPaymentRequest request)
    {
        // 1. Get payment request
        var paymentRequest = await _context.PaymentRequests
            .Include(p => p.Merchant)
            .ThenInclude(m => m.Account)
            .FirstOrDefaultAsync(p => p.PaymentRequestId == request.PaymentRequestId);

        if (paymentRequest == null) return NotFound("Payment request not found");

        // 2. Get customer account
        var customerAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == request.CustomerAccountNumber
                                   && a.Type == AccountType.Customer);

        if (customerAccount == null) return NotFound("Customer account not found");

        // 3. Check balance
        if (customerAccount.Balance < paymentRequest.Amount)
            return BadRequest("Insufficient balance");

        // 4. Create mock IPS callback
        var callbackData = new IpsCallbackDto
        {
            Reference = paymentRequest.Stan,
            TransactionId = $"IPS-TEST-{Guid.NewGuid()}",
            Amount = (decimal)paymentRequest.Amount,
            Currency = "RSD",
            Status = IpsPaymentStatus.Success,
            TransactionTimestamp = DateTime.UtcNow,
            PayerAccountNumber = customerAccount.AccountNumber,
            PayerName = customerAccount.AccountHolderName,
            Signature = "test-signature"
        };

        // 5. Process the callback (this will deduct from customer and credit merchant)
        var paymentService = HttpContext.RequestServices.GetRequiredService<IPaymentService>();
        await paymentService.ProcessIpsCallback(callbackData);

        return Ok(new
        {
            success = true,
            paymentRequestId = paymentRequest.PaymentRequestId,
            customerNewBalance = customerAccount.Balance,
            merchantNewBalance = paymentRequest.Merchant.Account.Balance
        });
    }
}

public class SimulateIpsPaymentRequest
{
    public Guid PaymentRequestId { get; set; }
    public string CustomerAccountNumber { get; set; } = string.Empty;
}