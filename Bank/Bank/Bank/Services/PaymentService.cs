using Bank.Clients;
using Bank.Contracts;
using Bank.Contracts.QR;
using Bank.Helpers;
using Bank.Models;
using Bank.Persistance;
using Bank.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bank.Services;

public class PaymentService : IPaymentService
{
    private readonly BankDbContext _context;
    private readonly string _frontendUrl;
    private IPSPClient _pspClient;
    private readonly Dictionary<string, string> _pspHmacKeys; // temp

    public PaymentService(BankDbContext context, IConfiguration config, IPSPClient pspClient)
    {
        _context = context;
        _frontendUrl = config["BankFrontendUrl"]!;
        _pspClient = pspClient;
        _pspHmacKeys = config.GetSection("PSPHmacKeys").Get<Dictionary<string, string>>() ?? new (); // temp
    }

    public async Task<InitializePaymentServiceResult> InitializePayment(PaymentInitRequest request, Guid pspId, string signature, DateTime timestamp, bool isQrPayment)
    {
        PSP? psp = await _context.PSPs.FindAsync(pspId);
        if (psp is null || !_pspHmacKeys.ContainsKey(pspId.ToString()))
        {
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidPsp, null);
        }

        Merchant? merchant = await _context.Merchants.FindAsync(request.MerchantId);
        if (merchant is null)
        {
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidMerchant, null);
        }

        var paymentRequest = PaymentServiceHelper.CreatePaymentRequest(request, pspId);
        _context.PaymentRequests.Add(paymentRequest);
        await _context.SaveChangesAsync();

        string paymentUrl = PaymentServiceHelper.BuildPaymentUrl(paymentRequest.PaymentRequestId, isQrPayment, _frontendUrl);

        return new InitializePaymentServiceResult(
            Result: InitializePaymentResult.Success,
            Response: new InitPaymentResponseDto(paymentRequest.PaymentRequestId, paymentUrl)
        );
    }

    public async Task<string> ExecuteCardPayment(Guid paymentRequestId, PayByCardRequest request)
    {
        using var dbTransaction = await _context.Database.BeginTransactionAsync();

        var paymentRequest = await _context.PaymentRequests.FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

        if (paymentRequest == null || paymentRequest.Status != PaymentRequestStatus.Pending)
        {
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        if (await HandleExpiredPayment(paymentRequest, dbTransaction))
        {
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        var card = await _context.DebitCards
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.CardNumber == request.CardNumber);

        if (card == null || !PaymentServiceHelper.IsValidCardPayment(request, paymentRequest, card))
        {
            return await FailPaymentAndNotify(paymentRequest, dbTransaction, paymentRequestId);
        }

        Merchant? merchant = await _context.Merchants
            .Include(c => c.Account)
            .FirstOrDefaultAsync(b => b.Id == paymentRequest.MerchantId);

        if (merchant?.Account is null)
        {
            return await FailPaymentAndNotify(paymentRequest, dbTransaction, paymentRequestId);
        }

        try
        {
            PaymentServiceHelper.TransferFunds(card.Account, merchant.Account, paymentRequest.Amount);
            var (globalTransactionId, acquirerTimestamp) = RecordSuccessfulTransaction(paymentRequestId);
            paymentRequest.Status = PaymentRequestStatus.Success;

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyPaymentSuccess(paymentRequest, globalTransactionId, acquirerTimestamp, merchant.Id);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            await RecordFailedTransaction(paymentRequestId);
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }
    }

    public async Task<GetPaymentRequestResponse> GetPaymentRequest(Guid paymentRequestId)
    {
        var paymentRequest = await _context.PaymentRequests
            .Where(p => p.PaymentRequestId == paymentRequestId)
            .Select(p => new GetPaymentRequestResponse(p.PaymentRequestId, p.Amount, p.Currency, p.Status, p.ExpiresAt, null))
            .FirstOrDefaultAsync();

        if (paymentRequest == null)
        {
            return new GetPaymentRequestResponse(paymentRequestId, 0, default, default, null, "Payment request not found.");
        }

        var response = PaymentServiceHelper.ValidatePaymentRequestStatus(paymentRequest);
        return response;
    }

    public async Task<QRPaymentResponseDto> GenerateQrPayment(Guid paymentRequestId)
    {
        var paymentRequest = await LoadPaymentRequest(paymentRequestId, includeMerchant: true);

        var ipsData = new QRIpsData(
            Currency: paymentRequest.Currency,
            Amount: paymentRequest.Amount,
            MerchantAccount: paymentRequest.Merchant.Account.AccountNumber.Replace("-", ""),
            MerchantName: paymentRequest.Merchant.Name,
            Purpose: "Placanje robe",
            PaymentCode: "289",
            Stan: paymentRequest.Stan
        );

        string payload = QRIpsPayloadGenerator.Generate(ipsData);
        string qrBase64 = QrImageGenerator.GenerateBase64(payload);

        return new QRPaymentResponseDto(
            paymentRequestId,
            qrBase64,
            paymentRequest.Status,
            paymentRequest.Stan,
            paymentRequest.ExpiresAt
        );
    }

    public async Task<ProcessQrPaymentResponse?> ProcessQrPayment(Guid paymentRequestId, string customerAccountNumber)
    {
        var paymentRequest = await LoadPaymentRequest(paymentRequestId, includeMerchant: true);

        if (paymentRequest.Status != PaymentRequestStatus.Pending)
        {
            return new ProcessQrPaymentResponse(paymentRequestId, paymentRequest.Status, string.Empty);
        }

        if (paymentRequest.ExpiresAt < DateTime.UtcNow)
        {
            paymentRequest.Status = PaymentRequestStatus.Expired;
            await _context.SaveChangesAsync();
            return new ProcessQrPaymentResponse(paymentRequestId, PaymentRequestStatus.Expired, string.Empty);
        }

        return await CompleteQrPayment(paymentRequestId, customerAccountNumber);
    }

    public async Task<QrPaymentStatusDto?> GetQrPaymentStatus(Guid paymentRequestId)
    {
        var paymentRequest = await _context.PaymentRequests
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

        if (paymentRequest is null)
            return null;

        var latestTransaction = paymentRequest.Transactions
            .OrderByDescending(t => t.AcquirerTimestamp)
            .FirstOrDefault();

        return new QrPaymentStatusDto(
            PaymentRequestId: paymentRequest.PaymentRequestId,
            Status: paymentRequest.Status,
            Amount: paymentRequest.Amount,
            Currency: paymentRequest.Currency,
            ExpiresAt: paymentRequest.ExpiresAt,
            TransactionId: latestTransaction?.Reference,
            CompletedAt: latestTransaction?.AcquirerTimestamp
        );
    }

    #region Private Helper Methods

    private async Task<bool> HandleExpiredPayment(PaymentRequest paymentRequest, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction)
    {
        if (paymentRequest.ExpiresAt >= DateTime.UtcNow)
            return false;

        paymentRequest.Status = PaymentRequestStatus.Expired;
        await _context.SaveChangesAsync();
        await dbTransaction.CommitAsync();
        return true;
    }

    private async Task<string> FailPaymentAndNotify(PaymentRequest paymentRequest, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction, Guid paymentRequestId)
    {
        paymentRequest.Status = PaymentRequestStatus.Failed;
        await _context.SaveChangesAsync();
        await dbTransaction.CommitAsync();
        return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
    }

    private (Guid GlobalTransactionId, DateTime AcquirerTimestamp) RecordSuccessfulTransaction(Guid paymentRequestId)
    {
        var globalTransactionId = Guid.NewGuid();
        var acquirerTimestamp = DateTime.UtcNow;

        _context.Transactions.Add(new Transaction
        {
            PaymentRequestId = paymentRequestId,
            GlobalTransactionId = globalTransactionId,
            AcquirerTimestamp = acquirerTimestamp,
            Status = TransactionStatus.Successful
        });

        return (globalTransactionId, acquirerTimestamp);
    }

    private async Task RecordFailedTransaction(Guid paymentRequestId)
    {
        _context.Transactions.Add(new Transaction
        {
            PaymentRequestId = paymentRequestId,
            GlobalTransactionId = Guid.NewGuid(),
            AcquirerTimestamp = DateTime.UtcNow,
            Status = TransactionStatus.Failed
        });
        await _context.SaveChangesAsync();
    }

    private async Task<string> NotifyPaymentSuccess(PaymentRequest paymentRequest, Guid globalTransactionId, DateTime acquirerTimestamp, string merchantId)
    {
        return await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
        {
            PaymentRequestId = paymentRequest.PaymentRequestId,
            PspPaymentId = paymentRequest.PspPaymentId,
            Stan = paymentRequest.Stan,
            GlobalTransactionId = globalTransactionId,
            AcquirerTimestamp = acquirerTimestamp,
            Status = TransactionStatus.Successful,
            MerchantId = merchantId,
            PspTimestamp = paymentRequest.PspTimestamp,
        });
    }

    private async Task<string> NotifyFailure(Guid paymentRequestId, TransactionStatus status)
    {
        var paymentRequest = await _context.PaymentRequests.FindAsync(paymentRequestId)
            ?? throw new Exception("Payment request not found.");

        return await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
        {
            PaymentRequestId = paymentRequestId,
            Stan = paymentRequest.Stan,
            GlobalTransactionId = Guid.NewGuid(),
            AcquirerTimestamp = DateTime.UtcNow,
            Status = status,
            MerchantId = paymentRequest.MerchantId,
            PspTimestamp = paymentRequest.PspTimestamp
        });
    }


    private async Task<ProcessQrPaymentResponse> CompleteQrPayment(Guid paymentRequestId, string customerAccountNumber)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            PaymentRequest paymentRequest = await LoadPaymentRequest(paymentRequestId, includeMerchant: true);

            if (paymentRequest.Status != PaymentRequestStatus.Pending)
                throw new Exception("Payment request not valid");

            Account customerAccount = await ResolveCustomerAccount(customerAccountNumber, paymentRequest.Amount);

            if (customerAccount.Balance < paymentRequest.Amount)
            {
                paymentRequest.Status = PaymentRequestStatus.Failed;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ProcessQrPaymentResponse(paymentRequestId, PaymentRequestStatus.Failed, string.Empty);
            }

            PaymentServiceHelper.TransferFunds(customerAccount, paymentRequest.Merchant.Account, paymentRequest.Amount);
            var (globalTransactionId, acquirerTimestamp) = RecordQrTransaction(paymentRequestId);
            paymentRequest.Status = PaymentRequestStatus.Success;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var redirectUrl = await TryNotifyPspAsync(paymentRequest, globalTransactionId, acquirerTimestamp);

            return new ProcessQrPaymentResponse(paymentRequestId, PaymentRequestStatus.Success, redirectUrl ?? string.Empty);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Account> ResolveCustomerAccount(string customerAccountNumber, double requiredAmount)
    {
        if (!string.IsNullOrEmpty(customerAccountNumber))
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == customerAccountNumber);
            if (account != null)
                return account;
        }

        var existingAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Type == AccountType.Customer && a.Balance >= requiredAmount);

        if (existingAccount != null)
            return existingAccount;

        var newAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = "123-456789-78",
            AccountHolderName = "Test Customer",
            Balance = 100000,
            Type = AccountType.Customer
        };
        _context.Accounts.Add(newAccount);
        return newAccount;
    }

    private (Guid GlobalTransactionId, DateTime AcquirerTimestamp) RecordQrTransaction(Guid paymentRequestId)
    {
        Guid globalTransactionId = Guid.NewGuid();
        var acquirerTimestamp = DateTime.UtcNow;

        _context.Transactions.Add(new Transaction
        {
            PaymentRequestId = paymentRequestId,
            GlobalTransactionId = globalTransactionId,
            AcquirerTimestamp = acquirerTimestamp,
            Status = TransactionStatus.Successful,
            Reference = $"QR-PAY-{Guid.NewGuid()}",
            Description = "QR payment completed via mobile app"
        });

        return (globalTransactionId, acquirerTimestamp);
    }

    private async Task<string?> TryNotifyPspAsync(PaymentRequest paymentRequest, Guid globalTransactionId, DateTime acquirerTimestamp)
    {
        try
        {
            return await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
            {
                PaymentRequestId = paymentRequest.PaymentRequestId,
                PspPaymentId = paymentRequest.PspPaymentId,
                Stan = paymentRequest.Stan,
                GlobalTransactionId = globalTransactionId,
                AcquirerTimestamp = acquirerTimestamp,
                Status = TransactionStatus.Successful,
                MerchantId = paymentRequest.MerchantId,
                PspTimestamp = paymentRequest.PspTimestamp,
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error notifying PSP: {ex.Message}");
            return null;
        }
    }

    private async Task<PaymentRequest> LoadPaymentRequest(Guid id, bool includeMerchant = false, bool includeTransactions = false)
    {
        IQueryable<PaymentRequest> query = _context.PaymentRequests;

        if (includeMerchant)
            query = query.Include(p => p.Merchant).ThenInclude(m => m.Account);

        if (includeTransactions)
            query = query.Include(p => p.Transactions);

        return await query.FirstOrDefaultAsync(p => p.PaymentRequestId == id)
            ?? throw new Exception("Payment request not found");
    }

    #endregion
}