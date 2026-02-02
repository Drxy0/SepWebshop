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
    [HttpGet("test")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "Bank API is running." });
    }

    [HttpPost("create-test-customer")]
    public async Task<IActionResult> CreateTestCustomer()
    {
        var customerAccount = new Account
        {
            AccountNumber = "105000000000000039",
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
            AccountNumber = "105000000000000029",
            AccountHolderName = "Test Shop d.o.o.",
            Balance = 0,
            Type = AccountType.Merchant
        };

        var merchant = new Merchant
        {
            Id = "MERCHANT_001",
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
}

public class SimulateIpsPaymentRequest
{
    public Guid PaymentRequestId { get; set; }
    public string CustomerAccountNumber { get; set; } = string.Empty;
}