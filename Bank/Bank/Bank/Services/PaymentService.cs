using Azure;
using Bank.Clients;
using Bank.Contracts;
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

    public async Task<InitializePaymentServiceResult> InitializePayment(
            PaymentInitRequest request,
            Guid pspId,
            string signature,
            DateTime timestamp,
            bool isQrPayment)
    {
        // Validate PSP
        PSP? psp = await _context.PSPs.FindAsync(pspId);
        if (psp is null)
        {
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidPsp, null);
        }

        // Get HMAC key
        if (!_pspHmacKeys.TryGetValue(pspId.ToString(), out var hmacKey))
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidPsp, null);

        // Validate signature
        string payload =
            $"merchantId={request.MerchantId}&amount={request.Amount}" +
            $"&currency={(int)request.Currency}&stan={request.Stan}" +
            $"&timestamp={timestamp:o}";

        // Validate merchant
        Merchant? merchant = await _context.Merchants.FindAsync(request.MerchantId);
        if (merchant is null)
        {
            return new InitializePaymentServiceResult(InitializePaymentResult.InvalidMerchant, null);
        }

        PaymentRequest paymentRequest = new PaymentRequest
        {
            PaymentRequestId = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            PspId = pspId,
            Amount = request.Amount,
            Currency = request.Currency,
            Stan = request.Stan,
            PspTimestamp = request.PspTimestamp,
            Status = PaymentRequestStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _context.PaymentRequests.Add(paymentRequest);
        await _context.SaveChangesAsync();

        string paymentUrl = isQrPayment
            ? $"{_frontendUrl}/payQr/{paymentRequest.PaymentRequestId}"
            : $"{_frontendUrl}/payCard/{paymentRequest.PaymentRequestId}";

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

        if (paymentRequest == null)
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);

        if (paymentRequest.Status != PaymentRequestStatus.Pending)
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);

        if (paymentRequest.ExpiresAt < DateTime.UtcNow)
        {
            paymentRequest.Status = PaymentRequestStatus.Expired;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        if (string.IsNullOrWhiteSpace(request.CVV)
            || request.CVV.Length != 3
            || !request.CVV.All(char.IsDigit))
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        // Card validation
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
            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);

        if (DebitCardHelper.IsCardExpired(card.ExpirationDate))
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        if (card.CVV != request.CVV)
        {
            paymentRequest.Status = PaymentRequestStatus.Failed;
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
        }

        if (card.Account.Balance < paymentRequest.Amount)
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
                p.PaymentRequestId,p.Amount,p.Currency,p.Status, p.ExpiresAt))
            .FirstOrDefaultAsync();

        if (paymentRequest == null)
        {
            throw new Exception("Payment request not found.");
        }

        if (paymentRequest.Status != PaymentRequestStatus.Pending)
        {
            throw new Exception("Payment request is not valid.");
        }

        if (paymentRequest.ExpiresAt < DateTime.UtcNow)
        {
            throw new Exception("Payment request expired.");
        }

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

        if (paymentRequest.Status != PaymentRequestStatus.Pending)
            throw new Exception("Payment request not valid");

        if (paymentRequest.ExpiresAt < DateTime.UtcNow)
            throw new Exception("Payment request expired");

        var ipsData = new QRIpsData(
            Currency: paymentRequest.Currency,
            Amount: paymentRequest.Amount,
            MerchantAccount: paymentRequest.Merchant.Account.AccountNumber.Replace("-", ""),
            MerchantName: paymentRequest.Merchant.Name,
            Purpose: "Placanje robe",
            PaymentCode: "289"
        );

        var payload = QRIpsPayloadGenerator.Generate(ipsData);
        var qrBase64 = QrImageGenerator.GenerateBase64(payload);

        return new QRPaymentResponseDto(paymentRequestId, qrBase64);
    }
}
