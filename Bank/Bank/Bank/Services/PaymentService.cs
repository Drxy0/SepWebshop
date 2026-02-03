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
    private readonly string _webShopSuccessUrl;


    public PaymentService(BankDbContext context, IConfiguration config, IPSPClient pspClient)
    {
        _context = context;
        _frontendUrl = config["BankFrontendUrl"]!;
        _pspClient = pspClient;
        _pspHmacKeys = config.GetSection("PSPHmacKeys").Get<Dictionary<string, string>>() ?? new(); // temp
        _webShopSuccessUrl = config["ApiSettings:WebShopSuccessUrl"] ?? throw new Exception("ApiSettings:WebShopSuccessUrl is missing from appsettings.json");
    }

    public async Task<InitializePaymentServiceResult> InitializePayment(PaymentInitRequest request, Guid pspId, string signature, DateTime timestamp, bool isQrPayment)
    {
        PSP? psp = await _context.PSPs.FindAsync(pspId);
        if (psp is null)
        {
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidPsp, null);
        }

        if (!TryGetPspHmacKey(pspId, out var hmacKey))
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidPsp, null);

        string payload = BuildSignaturePayload(request, timestamp);

        Merchant? merchant = await _context.Merchants.FindAsync(request.MerchantId);
        if (merchant is null)
        {
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidMerchant, null);
        }

        PaymentRequest paymentRequest = CreatePaymentRequest(request, pspId);

        _context.PaymentRequests.Add(paymentRequest);
        await _context.SaveChangesAsync();

        string paymentUrl = BuildFrontendPaymentUrl(paymentRequest.PaymentRequestId, isQrPayment);

        return new InitializePaymentServiceResult(
            Result: InitializePaymentResult.Success,
            Response: new InitPaymentResponseDto(paymentRequest.PaymentRequestId, paymentUrl)
        );
    }

    public async Task<string> ExecuteCardPayment(Guid paymentRequestId, PayByCardRequest request)
    {
        using var dbTransaction = await _context.Database.BeginTransactionAsync();

        PaymentRequest? paymentRequest = await _context.PaymentRequests
            .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

        if (paymentRequest == null || paymentRequest.Status != PaymentRequestStatus.Pending)
        {
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        if (paymentRequest.ExpiresAt < DateTime.UtcNow)
        {
            paymentRequest.Status = PaymentRequestStatus.Expired;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        if (!IsValidCvv(request.CVV))
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        if (!LuhnFormulaChecker.IsValidLuhn(request.CardNumber))
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        DebitCard? card = await _context.DebitCards
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.CardNumber == request.CardNumber);

        if (card == null)
        {
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        if (!IsCardPaymentValid(card, request, paymentRequest))
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        var merchant = await _context.Merchants
            .Include(c => c.Account)
            .FirstOrDefaultAsync(b => b.Id == paymentRequest.MerchantId);

        if (merchant?.Account is null)
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        try
        {
            card.Account.Balance -= paymentRequest.Amount;
            merchant.Account.Balance += paymentRequest.Amount;

            var globalTransactionId = Guid.NewGuid();
            var acquirerTimestamp = DateTime.UtcNow;

            _context.Transactions.Add(new Transaction
            {
                PaymentRequestId = paymentRequestId,
                GlobalTransactionId = globalTransactionId,
                AcquirerTimestamp = acquirerTimestamp,
                Status = TransactionStatus.Successful
            });

            paymentRequest.Status = PaymentRequestStatus.Success;

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            var redirectUrl = await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
            {
                PaymentRequestId = paymentRequestId,
                PspId = paymentRequest.PspId,
                PspPaymentId = paymentRequest.PspPaymentId,
                Stan = paymentRequest.Stan,
                GlobalTransactionId = globalTransactionId,
                AcquirerTimestamp = acquirerTimestamp,
                Status = TransactionStatus.Successful,
                MerchantId = merchant.Id,
                PspTimestamp = paymentRequest.PspTimestamp,
            });

            return redirectUrl;
        }
        catch
        {
            await dbTransaction.RollbackAsync();

            var globalTransactionId = Guid.NewGuid();
            var acquirerTimestamp = DateTime.UtcNow;

            _context.Transactions.Add(new Transaction
            {
                PaymentRequestId = paymentRequestId,
                GlobalTransactionId = globalTransactionId,
                AcquirerTimestamp = acquirerTimestamp,
                Status = TransactionStatus.Failed
            });

            await _context.SaveChangesAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }
    }

    private async Task<string> NotifyFailure(Guid paymentRequestId, TransactionStatus status)
    {
        PaymentRequest? paymentRequest = await _context.PaymentRequests.FindAsync(paymentRequestId);

        if (paymentRequest is null)
        {
            throw new Exception("Payment request not found.");
        }

        var redirectUrl = await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
        {
            PaymentRequestId = paymentRequestId,
            PspId = paymentRequest.PspId,
            PspPaymentId = paymentRequest.PspPaymentId,
            Stan = paymentRequest.Stan,
            GlobalTransactionId = Guid.NewGuid(),
            AcquirerTimestamp = DateTime.UtcNow,
            Status = status,
            MerchantId = paymentRequest.MerchantId,
            PspTimestamp = paymentRequest.PspTimestamp
        });

        return redirectUrl;
    }

    public async Task<PaymentRequestDto> GetPaymentRequest(Guid paymentRequestId)
    {
        PaymentRequestDto? paymentRequest = await _context.PaymentRequests
            .Where(p => p.PaymentRequestId == paymentRequestId)
            .Select(p => new PaymentRequestDto(
                p.PaymentRequestId, p.Amount, p.Currency, p.Status, p.ExpiresAt))
            .FirstOrDefaultAsync();

        if (paymentRequest == null)
        {
            throw new Exception("Payment request not found.");
        }

        ValidatePendingAndNotExpired(paymentRequest.Status, paymentRequest.ExpiresAt);

        return paymentRequest;
    }

    public async Task<QRPaymentResponseDto> GenerateQrPayment(Guid paymentRequestId)
    {
        PaymentRequest? paymentRequest = await _context.PaymentRequests
                .Include(p => p.Merchant)
                .ThenInclude(m => m.Account)
                .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

        if (paymentRequest == null)
            throw new Exception("Payment request not found");

        var ipsData = BuildIpsData(paymentRequest);

        var payload = QRIpsPayloadGenerator.Generate(ipsData);
        var qrBase64 = QrImageGenerator.GenerateBase64(payload);

        return new QRPaymentResponseDto(
            paymentRequestId,
            qrBase64,
            paymentRequest.Status,
            paymentRequest.Stan,
            paymentRequest.ExpiresAt
        );
    }

    public async Task<ProcessQrPaymentResponse> ProcessQrPayment(Guid paymentRequestId, string? customerAccountNumber = null)
    {
        var paymentRequest = await _context.PaymentRequests
            .Include(p => p.Merchant)
                .ThenInclude(m => m.Account)
            .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

        if (paymentRequest == null)
            throw new Exception("Payment request not found");

        if (paymentRequest.Status != PaymentRequestStatus.Pending)
        {
            return new ProcessQrPaymentResponse(paymentRequestId, paymentRequest.Status, null);
        }

        if (paymentRequest.ExpiresAt < DateTime.UtcNow)
        {
            paymentRequest.Status = PaymentRequestStatus.Expired;
            await _context.SaveChangesAsync();
            return new ProcessQrPaymentResponse(paymentRequestId, paymentRequest.Status, null);
        }

        return await CompleteQrPayment(paymentRequestId, customerAccountNumber);
    }

    private async Task<ProcessQrPaymentResponse> CompleteQrPayment(Guid paymentRequestId, string? customerAccountNumber)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var paymentRequest = await _context.PaymentRequests
                .Include(p => p.Merchant)
                    .ThenInclude(m => m.Account)
                .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

            if (paymentRequest is null || paymentRequest.Status != PaymentRequestStatus.Pending)
            {
                throw new Exception("Payment request not valid");
            }

            Account? customerAccount = null;
            if (!string.IsNullOrEmpty(customerAccountNumber))
            {
                customerAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == customerAccountNumber);
            }

            if (customerAccount == null)
            {
                customerAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Type == AccountType.Customer && a.Balance >= paymentRequest.Amount);

                if (customerAccount == null)
                {
                    customerAccount = CreateTestCustomerAccount();
                    _context.Accounts.Add(customerAccount);
                }
            }

            if (customerAccount.Balance < paymentRequest.Amount)
            {
                paymentRequest.Status = PaymentRequestStatus.Failed;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ProcessQrPaymentResponse(paymentRequestId, paymentRequest.Status, null);
            }

            customerAccount.Balance -= paymentRequest.Amount;
            paymentRequest.Merchant.Account.Balance += paymentRequest.Amount;

            var globalTransactionId = Guid.NewGuid();
            var acquirerTimestamp = DateTime.UtcNow;

            _context.Transactions.Add(new Transaction
            {
                PaymentRequestId = paymentRequestId,
                GlobalTransactionId = globalTransactionId,
                AcquirerTimestamp = acquirerTimestamp,
                Status = TransactionStatus.Successful,
                Reference = $"QR-PAY-{Guid.NewGuid()}",
                Description = $"QR payment completed via mobile app"
            });

            paymentRequest.Status = PaymentRequestStatus.Success;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            try
            {
                await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
                {
                    PaymentRequestId = paymentRequestId,
                    PspId = paymentRequest.PspId,
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
            }

            return new ProcessQrPaymentResponse(paymentRequestId, paymentRequest.Status, _webShopSuccessUrl);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<QrPaymentStatusDto> GetQrPaymentStatus(Guid paymentRequestId)
    {
        PaymentRequest? paymentRequest = await _context.PaymentRequests
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

        if (paymentRequest == null)
            throw new Exception("Payment request not found.");

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

    // ----------------- extracted helpers -----------------

    private bool TryGetPspHmacKey(Guid pspId, out string key)
        => _pspHmacKeys.TryGetValue(pspId.ToString(), out key!);

    private static string BuildSignaturePayload(PaymentInitRequest request, DateTime timestamp)
        => $"merchantId={request.MerchantId}&amount={request.Amount}" +
           $"&currency={(int)request.Currency}&stan={request.Stan}" +
           $"&timestamp={timestamp:o}";

    private static PaymentRequest CreatePaymentRequest(PaymentInitRequest request, Guid pspId)
        => new()
        {
            PaymentRequestId = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            PspId = pspId,
            PspPaymentId = request.PspPaymentId,
            Amount = request.Amount,
            Currency = request.Currency,
            Stan = request.Stan,
            PspTimestamp = request.PspTimestamp,
            Status = PaymentRequestStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

    private string BuildFrontendPaymentUrl(Guid paymentRequestId, bool isQrPayment)
        => isQrPayment
            ? $"{_frontendUrl}/pay/qr/{paymentRequestId}"
            : $"{_frontendUrl}/pay/card/{paymentRequestId}";

    private static bool IsValidCvv(string cvv)
        => !string.IsNullOrWhiteSpace(cvv) &&
           cvv.Length == 3 &&
           cvv.All(char.IsDigit);

    private static bool IsCardPaymentValid(DebitCard card, PayByCardRequest request, PaymentRequest paymentRequest)
    {
        string requestExpiry = $"{request.ExpiryMonth:D2}/{request.ExpiryYear:D2}";

        return !DebitCardHelper.IsCardExpired(card.ExpirationDate) &&
               card.ExpirationDate == requestExpiry &&
               card.CVV == request.CVV &&
               card.Account.Balance >= paymentRequest.Amount;
    }

    private static void ValidatePendingAndNotExpired(PaymentRequestStatus status, DateTime expiresAt)
    {
        if (status != PaymentRequestStatus.Pending)
            throw new Exception("Payment request is not valid.");

        if (expiresAt < DateTime.UtcNow)
            throw new Exception("Payment request expired.");
    }

    private static QRIpsData BuildIpsData(PaymentRequest paymentRequest)
        => new(
            Currency: paymentRequest.Currency,
            Amount: paymentRequest.Amount,
            MerchantAccount: paymentRequest.Merchant.Account.AccountNumber.Replace("-", ""),
            MerchantName: paymentRequest.Merchant.Name,
            Purpose: "Placanje robe",
            PaymentCode: "289",
            Stan: paymentRequest.Stan
        );

    private static Account CreateTestCustomerAccount()
        => new()
        {
            Id = Guid.NewGuid(),
            AccountNumber = "123-456789-78",
            AccountHolderName = "Test Customer",
            Balance = 100000,
            Type = AccountType.Customer
        };
}
